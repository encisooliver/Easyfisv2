﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace easyfis.ModifiedApiControllers
{
    public class ApiMstCompanyController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ============
        // List Company
        // ============
        [Authorize, HttpGet, Route("api/company/list")]
        public List<Entities.MstCompany> ListCompany()
        {
            var companies = from d in db.MstCompanies.OrderByDescending(d => d.Company)
                            select new Entities.MstCompany
                            {
                                Id = d.Id,
                                Company = d.Company,
                                Address = d.Address,
                                TaxNumber = d.TaxNumber,
                                IsLocked = d.IsLocked,
                                CreatedBy = d.MstUser.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = d.MstUser1.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            return companies.ToList();
        }

        // ==============
        // Detail Company
        // ==============
        [Authorize, HttpGet, Route("api/company/detail/{id}")]
        public Entities.MstCompany DetailCompany(String id)
        {
            var company = from d in db.MstCompanies
                          where d.Id == Convert.ToInt32(id)
                          select new Entities.MstCompany
                          {
                              Id = d.Id,
                              Company = d.Company,
                              Address = d.Address,
                              ContactNumber = d.ContactNumber,
                              TaxNumber = d.TaxNumber,
                              IsLocked = d.IsLocked,
                              CreatedBy = d.MstUser.FullName,
                              CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                              UpdatedBy = d.MstUser1.FullName,
                              UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                          };

            if (company.Any())
            {
                return company.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ===========
        // Add Company
        // ===========
        [Authorize, HttpPost, Route("api/company/add")]
        public HttpResponseMessage AddCompany()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;
                    var currentBranchId = currentUser.FirstOrDefault().BranchId;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            Data.MstCompany newCompany = new Data.MstCompany
                            {
                                Company = "NA",
                                Address = "NA",
                                ContactNumber = "NA",
                                TaxNumber = "NA",
                                IsLocked = false,
                                CreatedById = currentUserId,
                                CreatedDateTime = DateTime.Now,
                                UpdatedById = currentUserId,
                                UpdatedDateTime = DateTime.Now
                            };

                            db.MstCompanies.InsertOnSubmit(newCompany);
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK, newCompany.Id);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add company.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ============
        // Lock Company
        // ============
        [Authorize, HttpPut, Route("api/company/lock/{id}")]
        public HttpResponseMessage LockCompany(Entities.MstCompany objCompany, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (company.Any())
                            {
                                if (!company.FirstOrDefault().IsLocked)
                                {
                                    var lockCompany = company.FirstOrDefault();
                                    lockCompany.Company = objCompany.Company;
                                    lockCompany.Address = objCompany.Address;
                                    lockCompany.ContactNumber = objCompany.ContactNumber;
                                    lockCompany.TaxNumber = objCompany.TaxNumber;
                                    lockCompany.IsLocked = true;
                                    lockCompany.UpdatedById = currentUserId;
                                    lockCompany.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These company details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These company details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock company.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ==============
        // Unlock Company
        // ==============
        [Authorize, HttpPut, Route("api/company/unlock/{id}")]
        public HttpResponseMessage UnlockCompany(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (company.Any())
                            {
                                if (company.FirstOrDefault().IsLocked)
                                {
                                    var unlockCompany = company.FirstOrDefault();
                                    unlockCompany.IsLocked = false;
                                    unlockCompany.UpdatedById = currentUserId;
                                    unlockCompany.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These company details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These company details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock company.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // ==============
        // Delete Company
        // ==============
        [Authorize, HttpDelete, Route("api/company/delete/{id}")]
        public HttpResponseMessage DeleteCompany(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var userForms = from d in db.MstUserForms
                                    where d.UserId == currentUserId
                                    && d.SysForm.FormName.Equals("CompanyList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var company = from d in db.MstCompanies
                                          where d.Id == Convert.ToInt32(id)
                                          select d;

                            if (company.Any())
                            {
                                if (!company.FirstOrDefault().IsLocked)
                                {
                                    db.MstCompanies.DeleteOnSubmit(company.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete company if the current company record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These company details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete company.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this company page.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
