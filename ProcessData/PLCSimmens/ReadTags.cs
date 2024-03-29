﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp7;

namespace ProcessData.PLCSimmens
{
    public class ReadTags
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

        public ReadTags(string IP, int Rack, int Slot, out int Result)
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
        }

        public string ReadAreaByteToString(int DbNumber, int Start, int Amount)
        {
            string Result = "";
            try
            {
                Client.ReadArea(S7Area.DB, DbNumber, Start, Amount, S7WordLength.Byte, Buffer);
                for (int i = 0; i <= Amount - 1; i++)
                {
                    Result = Result + (Convert.ToString((char)Buffer[i]));
                }
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

        public List<int> ReadAreaIntToListInt(int DbNumber, int Start, int Amount)
        {
            List<int> Result = new List<int>();
            try
            {
                Client.ReadArea(S7Area.DB, DbNumber, Start, Amount, S7WordLength.DInt, Buffer);
                for (int i = 0; i <= Amount - 1; i = i + 2)
                {
                    Result.Add(Buffer[i] * 256 + Buffer[i + 1]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Result;
        }

        public List<int> ReadQuantity()
        {
            List<int> listQuantity = new List<int>();
            try
            {
                S7MultiVar Reader = new S7MultiVar(Client);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 2, 2, 2, ref DB_A);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 3, 0, 2, ref DB_B);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 4, 0, 2, ref DB_C);
                var Result = Reader.Read();
                var intOK = Sharp7.S7.GetDIntAt(DB_A, 0);
                var intNG = Sharp7.S7.GetDIntAt(DB_B, 0);
                var intRW = Sharp7.S7.GetDIntAt(DB_C, 0);
                listQuantity.Add(DB_A[0] * 256 + DB_A[1]);
                listQuantity.Add(DB_B[0] * 256 + DB_B[1]);
                listQuantity.Add(DB_C[0] * 256 + DB_C[1]);
                return listQuantity;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<int> ReadQuantityProduced()
        {
            List<int> listQuantity = new List<int>();
            try
            {
                S7MultiVar Reader = new S7MultiVar(Client);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 6, 4, 2, ref DB_A);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 6, 6, 2, ref DB_B);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.DInt, 6, 8, 2, ref DB_C);
                var Result = Reader.Read();
                var intOK = Sharp7.S7.GetDIntAt(DB_A, 2);
                var intNG = Sharp7.S7.GetDIntAt(DB_B, 0);
                var intRW = Sharp7.S7.GetDIntAt(DB_C, 0);
                listQuantity.Add(intOK);
                listQuantity.Add(intNG);
                listQuantity.Add(intRW);
                return listQuantity;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, bool> ReadStatusPLC()
        {
            Dictionary<string, bool> keyValuePairs = new Dictionary<string, bool>();
            keyValuePairs.Add(TagSubcribed.FlagKT, false);
            keyValuePairs.Add(TagSubcribed.IsReset, false);
            keyValuePairs.Add(TagSubcribed.OnOFF, false);
            keyValuePairs.Add(TagSubcribed.WriteReadyStart, false);
            try
            {
                S7MultiVar Reader = new S7MultiVar(Client);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.Byte, 7, 2048, 1, ref DB_A);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.Byte, 181, 208, 1, ref DB_B);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.Byte, 9, 0, 1, ref DB_C);
                Reader.Add((int)S7Area.DB, (int)S7WordLength.Byte, 181, 204, 1, ref DB_D);
                var Result = Reader.Read();
                keyValuePairs[TagSubcribed.FlagKT] = Sharp7.S7.GetBitAt(DB_A, 0, 0);
                keyValuePairs[TagSubcribed.IsReset] = Sharp7.S7.GetBitAt(DB_B, 0, 0);
                keyValuePairs[TagSubcribed.OnOFF] = Sharp7.S7.GetBitAt(DB_C, 0, 2);
                keyValuePairs[TagSubcribed.WriteReadyStart] = Sharp7.S7.GetBitAt(DB_D, 0, 0);

                return keyValuePairs;
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
