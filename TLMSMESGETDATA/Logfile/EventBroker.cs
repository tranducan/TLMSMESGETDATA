using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TLMSMESGETDATA
{
    /// <summary>
    /// event 중개자입니다.
    /// </summary>
    public static class EventBroker
    {
        /// <summary>
        /// Event 종류 별로 enum에 ID를 추가해주세요
        /// </summary>
        public enum EventID
        {
            etLog,
            etFileTimer,
            etStartProgram,
            etCheckDate,
            etFileSync,
            etUpdateMe,
            etNull
        }
        public delegate void EventObserver(EventID id, EventParam args); // event 수신 용 Delegate 정의
        private static List<Map2<EventID, EventObserver> > Observers = null; // event 수신 용 Delegate List
        private static Queue<Map3<bool, EventID, EventObserver>> ObserversAddRemoveQueue = null;// event 수신 용 Delegate List 1차 버퍼, 2차 버퍼에 추가 또는 삭제되기 위한 버퍼임

        private static Queue<Map2<EventID, EventParam> > AsyncEventAddQueue = null; // Event 전송 1차 버퍼, 2차 버퍼에 추가되기 위한 버퍼임
        private static Queue<Map2<EventID, EventParam>> AsyncEventReadyQueue = null; // Event 전송 2차 버퍼, 이벤트 전송 대기중인 버퍼

        private static Queue<Map2<bool, EventParamTimer>> AsyncTimerEventAddRemoveQueue = null;// Time Event 전송 1차 버퍼, 2차 버퍼에 추가 또는 삭제되기 위한 버퍼임
        private static List<EventParamTimer> AsyncTimerEventReadyQueue = null;// Time Event 전송 2차 버퍼, 이벤트 전송 대기중인 버퍼

        private static ReaderWriterLockSlim cacheLock = null; // Event 수신용 Delegate 배열의 Thread Safe 용

        private static Thread m_eventThread = null;
        private static bool m_eventThreadRunFlag = false;

        private static Thread m_timerThread = null;
        private static bool m_timerThreadRunFlag = false;

        private static bool m_relesedBlocker = false;
        static EventBroker()
        {
            Observers = new List<Map2<EventID, EventObserver>>();
            ObserversAddRemoveQueue = new Queue<Map3<bool,EventID, EventObserver>>();

            AsyncEventAddQueue = new Queue<Map2<EventID, EventParam> >();
            AsyncEventReadyQueue = new Queue<Map2<EventID, EventParam>>();

            AsyncTimerEventAddRemoveQueue = new Queue<Map2<bool, EventParamTimer>>();
            AsyncTimerEventReadyQueue = new List<EventParamTimer>();

            cacheLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 프로그램 종료시 꼭 릴리즈를 호출해주세요
        /// </summary>
        public static void Relase()
        {
            m_relesedBlocker = true;
            m_eventThreadRunFlag = false;
            m_timerThreadRunFlag = false;
            StopEventThread();
            StopTimerThread();
            lock (AsyncEventAddQueue)
            {
                AsyncEventAddQueue.Clear();
            }
            lock (AsyncTimerEventAddRemoveQueue)
            {
                AsyncTimerEventAddRemoveQueue.Clear();
            }
            cacheLock.EnterWriteLock();
            try
            {
                Observers.Clear();
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }

            AsyncEventReadyQueue.Clear();
            AsyncTimerEventReadyQueue.Clear();
        }

        /// <summary>
        /// Event를 수신할 함수를 추가합니다.
        /// </summary>
        /// <param name="id">수신하고자하는 event id 종류입니다</param>
        /// <param name="observer">Event를 수신하고자하는 함수 delegate 입니다</param>
        public static void AddObserver(EventID id, EventObserver observer)
        {
            lock (ObserversAddRemoveQueue)
            {
                foreach (Map3<bool, EventID, EventObserver> set in ObserversAddRemoveQueue)
                {
                    if (set.d2 == id && set.d3.Equals(observer))
                        return;
                }
                Map3<bool, EventID, EventObserver> newEvent = new Map3<bool, EventID, EventObserver>(true, id, observer);
                ObserversAddRemoveQueue.Enqueue(newEvent);
            }
        }
        /// <summary>
        /// Evnet를 수신중인 함수 delegate를 삭제합니다.
        /// </summary>
        /// <param name="id">수신중인 event id 종류입니다</param>
        /// <param name="observer">수신중인 함수 delegate입니다</param>
        public static void RemoveObserver(EventID id, EventObserver observer)
        {
            lock (ObserversAddRemoveQueue)
            {
                Map3<bool, EventID, EventObserver> newEvent = new Map3<bool, EventID, EventObserver>(false, id, observer);
                ObserversAddRemoveQueue.Enqueue(newEvent);
            }
        }

        /// <summary>
        /// Event를 동기적으로 전송합니다
        /// </summary>
        /// <param name="id">전송하고자하는 event id 종류입니다</param>
        /// <param name="args">전송하고자하는 Data입니다</param>
        public static void Send(EventID id, EventParam args)
        {
            cacheLock.EnterReadLock();
            try
            {
                foreach (Map2<EventID, EventObserver> set in Observers)
                {
                    if (set.d1 == id)
                    {
                        lock (set)
                        {
                            set.d2(id, args);
                        }
                    }
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Event를 비동기적으로 전송합니다. 별도의 Thread에서 Delegate를 호출합니다.
        /// </summary>
        /// <param name="id">전송하고자하는 event id 종류입니다</param>
        /// <param name="args">전송하고자하는 Data입니다</param>
        public static void AsyncSend(EventID id, EventParam args)
        {
            if (m_eventThread == null)
                StartEventThread();

            lock (AsyncEventAddQueue)
            {
                AsyncEventAddQueue.Enqueue(new Map2<EventID, EventParam>(id, args));
                Monitor.Pulse(AsyncEventAddQueue);
            }
        }

        /// <summary>
        /// 일정시간뒤에 Event를 전달하거나, 일정시간 간격으로 반복해서 Event를 전송함
        /// </summary>
        /// <param name="id">전송하고자하는 event id 종류입니다</param>
        /// <param name="args">전송하고자하는 Data입니다</param>
        /// <param name="milliSecond">지연시간 또는 반복 간격입니다</param>
        /// <param name="repeat">반복 여부입니다</param>
        public static void AddTimeEvent(EventID id, EventParam args, int milliSecond, bool repeat = false)
        {
            if (m_timerThread == null)
                StartTimerThread();

            lock (AsyncTimerEventAddRemoveQueue)
            {
                AsyncTimerEventAddRemoveQueue.Enqueue(new Map2<bool, EventParamTimer>(true, new EventParamTimer(id,args,milliSecond,repeat)));
                Monitor.Pulse(AsyncTimerEventAddRemoveQueue);
            }
        }
        /// <summary>
        /// 동작중인 TimeEvent를 삭제합니다
        /// </summary>
        /// <param name="id">event id 종류입니다</param>
        /// <param name="args">AddTimeEvent함수에서 사용한 EventParam 객체입니다</param>
        public static void RemoveTimeEvent(EventID id, EventParam args)
        {
            if (m_timerThread == null)
                return;

            lock (AsyncTimerEventAddRemoveQueue)
            {
                AsyncTimerEventAddRemoveQueue.Enqueue(new Map2<bool, EventParamTimer>(false, new EventParamTimer(id, args, 0)));
                Monitor.Pulse(AsyncTimerEventAddRemoveQueue);
            }
        }
        ////////////////////////////////////////////////////////////////////////////
        private static void StartEventThread()
        {
            if (m_relesedBlocker)
                return;
            if (m_eventThread != null)
                StopEventThread();
            m_eventThread = new Thread(new ThreadStart(ThreadProcEvent));
            m_eventThread.Start();
        }

        private static void StopEventThread()
        {
            if (m_eventThread == null)
                return;
            m_eventThreadRunFlag = false;
            if (AsyncEventAddQueue.Count == 0)
            {
                lock (AsyncEventAddQueue)
                {
                    Monitor.Pulse(AsyncEventAddQueue);
                }
            }
            m_eventThread.Interrupt();
            m_eventThread.Join();
            m_eventThread = null;
        }

        private static void ObserverStateCheck()
        {
            lock (ObserversAddRemoveQueue)
            {
                if (ObserversAddRemoveQueue.Count > 0)
                {
                    try
                    {
                        cacheLock.EnterWriteLock();
                        while (ObserversAddRemoveQueue.Count > 0)
                        {
                            Map3<bool, EventID, EventObserver> set3 = ObserversAddRemoveQueue.Dequeue();
                            Map2<EventID, EventObserver> result = null;
                            foreach (Map2<EventID, EventObserver> set2 in Observers)
                            {
                                if (set3.d2 == set2.d1 && set3.d3.Equals(set2.d2))
                                {
                                    result = set2;
                                    break;
                                }
                            }
                            if (set3.d1 == true)
                            {
                                if (result == null)
                                    Observers.Add(new Map2<EventID, EventObserver>(set3.d2, set3.d3));
                            }
                            else
                            {
                                if (result != null)
                                    Observers.Remove(result);
                            }
                        }
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
            }
        }
        private static void ThreadProcEvent()
        {
            m_eventThreadRunFlag = true;
            while (m_eventThreadRunFlag)
            {
                try
                {
                    lock (AsyncEventAddQueue)
                    {
                        if (AsyncEventAddQueue.Count == 0)
                        {
                            Monitor.Wait(AsyncEventAddQueue,1000);
                        }
                        while (AsyncEventAddQueue.Count > 0)
                            AsyncEventReadyQueue.Enqueue(AsyncEventAddQueue.Dequeue());
                    }

                    if (ObserversAddRemoveQueue.Count > 0)
                        ObserverStateCheck();

                    while (AsyncEventReadyQueue.Count > 0)
                    {
                        Map2<EventID, EventParam> args = AsyncEventReadyQueue.Dequeue();
                        Send(args.d1, args.d2);
                    }
                }
                catch (ThreadInterruptedException /*ex*/) { }
                catch (Exception ex)
                {
                    if(m_eventThreadRunFlag)
                        AsyncSend(EventID.etLog, new EventParam(null, 0, "Event Broker : Exception - " + ex.Message));
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        private static void StartTimerThread()
        {
            if (m_relesedBlocker)
                return;
            if (m_timerThread != null)
                StopTimerThread();
            m_timerThread = new Thread(new ThreadStart(ThreadProcTimer));
            m_timerThread.Start();
        }

        private static void StopTimerThread()
        {
            if (m_timerThread == null)
                return;
            m_timerThreadRunFlag = false;
            lock (AsyncTimerEventAddRemoveQueue)
            {
                Monitor.Pulse(AsyncTimerEventAddRemoveQueue);
            }
            m_timerThread.Interrupt();
            m_timerThread.Join();
            m_timerThread = null;
        }
        private static void ThreadProcTimer()
        {
            m_timerThreadRunFlag = true;
            int minDurationTime = Int32.MaxValue;
            List<EventParamTimer> eventFinishs = new List<EventParamTimer>();
            while (m_timerThreadRunFlag)
            {
                try
                {
                    lock (AsyncTimerEventAddRemoveQueue)
                    {
                        if (AsyncTimerEventAddRemoveQueue.Count == 0 && minDurationTime > 0)
                            Monitor.Wait(AsyncTimerEventAddRemoveQueue, minDurationTime);

                        while (AsyncTimerEventAddRemoveQueue.Count > 0)
                        {
                            Map2<bool, EventParamTimer> arg = AsyncTimerEventAddRemoveQueue.Dequeue();
                            if (arg.d1)
                            {// add
                                AsyncTimerEventReadyQueue.Add(arg.d2);
                            }
                            else
                            {// delete
                                foreach (EventParamTimer ept in AsyncTimerEventReadyQueue)
                                {
                                    if (ept.ID == arg.d2.ID && ept.Param.Equals(arg.d2.Param) == true)
                                    {
                                        AsyncTimerEventReadyQueue.Remove(ept);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (ObserversAddRemoveQueue.Count > 0)
                        ObserverStateCheck();

                    eventFinishs.Clear();
                    foreach (EventParamTimer ept in AsyncTimerEventReadyQueue)
                    {
                        if (ept.IsNeedRun() == true)
                        {
                            Send(ept.ID, ept.Param);
                            if (!ept.Repeat)
                                eventFinishs.Add(ept);
                            else
                                ept.SetNextTickCount();
                        }
                    }
                    foreach (EventParamTimer ept in eventFinishs)
                        AsyncTimerEventReadyQueue.Remove(ept);

                    minDurationTime = Int32.MaxValue;
                    foreach (EventParamTimer ept in AsyncTimerEventReadyQueue)
                    {
                        if (minDurationTime > ept.GetNextTickCount())
                            minDurationTime = ept.GetNextTickCount();
                    }
                }
                catch (ThreadInterruptedException /*ex*/){}
                catch (Exception ex)
                {
                    if(m_timerThreadRunFlag)
                        AsyncSend(EventID.etLog, new EventParam(null, 0, "Event Broker : Exception - " + ex.Message));
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event 전송시 같이 보내고자하는 data입니다.
        /// </summary>
        public class EventParam : EventArgs
        {
            private object sender = null;
            private int paramInt = 0;
            private string paramString = null;
            private object paramObj = null;
            public EventParam(object _sender, int _paramInt, string _paramString = null, object _paramObj = null)
            {
                sender = _sender;
                paramInt = _paramInt;
                paramString = _paramString;
                paramObj = _paramObj;
            }
            public int ParamInt { get { return paramInt; } }
            public string ParamString { get { return paramString; } }
            public object ParamObj { get { return paramObj; } }
            public object Sender { get { return sender; } }
            public bool ParamObjEqual(object obj)
            {
                if (paramObj == null || obj == null)
                    return false;
                return Object.ReferenceEquals(paramObj, obj);
            }
        }

        private class EventParamTimer
        {
            EventID eventID = EventID.etNull;
            EventParam eventParam = null;
            int delayms = 0;
            bool repeatFlag = false;
            int nextEventTick = 0;
            const int int32Half = Int32.MaxValue / 2;
            public EventParamTimer(EventID id, EventParam eParam, int millisecond, bool repeat = false)
            {
                if (millisecond > 86400000)
                    throw new Exception("The milliSecond range over(1~86,400,000)");
                eventID = id;
                eventParam = eParam;
                delayms = millisecond;
                repeatFlag = repeat;
                SetNextTickCount();
            }
            public EventParam Param { get { return eventParam; } }
            public bool Repeat { get { return repeatFlag; } }
            public EventID ID { get { return eventID; } }
            public int GetNextTickCount()
            {
                int duration = nextEventTick - Environment.TickCount;
                if (duration < 0)
                    duration = 0;
                return duration;
            }
            public void SetNextTickCount()
            {
                nextEventTick = Environment.TickCount + delayms;
            }
            public bool IsNeedRun()
            {
                int duration = nextEventTick - Environment.TickCount;
                return (duration < 1);
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Map2<T1,T2>
    {
        public T1 d1 = default(T1);
        public T2 d2 = default(T2);
        public Map2() { }
        public Map2(T1 _d1, T2 _d2)
        {
            d1 = _d1;
            d2 = _d2;
        }
    }
    public class Map3<T1, T2, T3>
    {
        public T1 d1 = default(T1);
        public T2 d2 = default(T2);
        public T3 d3 = default(T3);
        public Map3() { }
        public Map3(T1 _d1, T2 _d2, T3 _d3)
        {
            d1 = _d1;
            d2 = _d2;
            d3 = _d3;
        }
    }
}
