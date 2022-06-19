using EFTechlinkMesDb.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTechlinkMesDb.Implementation
{
    public class DataContextAction : IDataContextAction
    {
        public bool Insert(PQCMesData pQCMesData)
        {
            if (pQCMesData is null)
                throw new ArgumentNullException("PQCMESData is null");

            using (var context = new EFTechLinkMESModel())
            {
                try
                {
                    context.PQCMesDatas.Add(pQCMesData);
                    var result = context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw ex;
                }
                return true;
            }
        }

        public PQCMesData Select(string Model)
        {
            throw new NotImplementedException();
        }

        public List<PQCMesData> SelectTopNotProcess(string flag, int topQuery)
        {
            List<PQCMesData> pQCMesDatas = new List<PQCMesData>();
            using (var context = new EFTechLinkMESModel())
            {
                pQCMesDatas = context.PQCMesDatas
                    .Where(d => d.Flag == flag || d.Flag == null)
                    .OrderBy(o => o.InspectDateTime)
                    .Take(topQuery)
                    .ToList();
            }

            return pQCMesDatas;
        }

        bool IDataContextAction.UpdateFlagTransferingSuccessful(long ID)
        {
           
            using (var context = new EFTechLinkMESModel())
            {
                var dataUpdated = context.PQCMesDatas
                    .Where(d => d.PQCMesDataId == ID)
                    .FirstOrDefault();
                try
                {
                    if (dataUpdated != null)
                    {
                        dataUpdated.Flag = "Success";
                        context.PQCMesDatas.Attach(dataUpdated);
                        context.Entry(dataUpdated).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }

            return true;
        }
    }
}
