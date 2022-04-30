using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp7;

namespace ProcessData.PLCSimmens
{
    public class WriteTags
    {
        private S7Client Client;
        private string _IP;
        private int _Rack;
        private int _Slot;
        private byte[] Buffer = new byte[65536];
        private byte[] DB_A = new byte[1024];
        private byte[] DB_B = new byte[1024];
        private byte[] DB_C = new byte[1024];
        private byte[] DB_D = new byte[1024];
        public string ConnectionMessage;
        public WriteTags(string IP, int Rack, int Slot, out int Result)
        {
            Client = new S7Client();
            _IP = IP;
            _Rack = Rack;
            _Rack = Slot;
            Client.ConnTimeout = 500;
            Client.RecvTimeout = 500;
            Client.SendTimeout = 500;
            Result = Client.ConnectTo(_IP, _Rack, _Slot);
            if (Result == 0)
            {
                ConnectionMessage = Client.ErrorText(Result);
            }
            var StrMessage = Client.ErrorText(0);
        }

        public int Diconnect()
        {
            int Result = -1;
            try
            {
                Result = Client.Disconnect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        public int isConnectionPLC()
        {
            int Result = -1;
            try
            {
                Result = Client.ConnectTo(_IP, _Rack, _Slot);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        public void WriteQRMESto(int DbNumber, int Start, int Amount, string Text)
        {
            try
            {
                byte[] Buffer = new byte[65536];
                Sharp7.S7.SetCharsAt(Buffer, 0, Text);
                Client.WriteArea(S7Area.DB, DbNumber, Start, Amount, S7WordLength.Byte, Buffer);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public void WriteMQCProducedQuantitytoPLC(Int16 OutputQty, Int16 NGQty, Int16 RWQty)
        {
            try
            {
                byte[] BufferOP = new byte[2];
                byte[] BufferNG = new byte[2];
                byte[] BufferRW = new byte[2];

                Sharp7.S7.SetWordAt(BufferOP, 0, ushort.Parse(OutputQty.ToString()));
                Sharp7.S7.SetWordAt(BufferNG, 0, ushort.Parse(NGQty.ToString()));
                Sharp7.S7.SetWordAt(BufferRW, 0, ushort.Parse(RWQty.ToString()));
                int Result;
                Result = Client.WriteArea(S7Area.DB, 181, 210, 2, S7WordLength.Byte, BufferOP);
                Result = Client.WriteArea(S7Area.DB, 181, 212, 2, S7WordLength.Byte, BufferNG);
                Result = Client.WriteArea(S7Area.DB, 181, 214, 2, S7WordLength.Byte, BufferRW);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void WritebittoPLC(bool value, int db, int start, int size)
        {
            try
            {
                byte[] buffer = new byte[1];
                Sharp7.S7.SetBitAt(buffer, 0, 0, value);
                Client.DBWrite(db, start, size, buffer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void WriteDinttoPLC(int value, int db, int start, int size)
        {
            try
            {
                byte[] buffer = new byte[2];
                Sharp7.S7.SetWordAt(buffer, 0, ushort.Parse(value.ToString()));
                Client.WriteArea(S7Area.DB, db, start, 2, S7WordLength.Byte, buffer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
