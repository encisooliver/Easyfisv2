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
    public class ApiTrnArticlePriceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ==================
        // List Article Price
        // ==================
        [Authorize, HttpGet, Route("api/articlePrice/list/{startDate}/{endDate}")]
        public List<Entities.TrnArticlePrice> ListArticlePrice(String startDate, String endDate)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var articlePrices = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                                where d.BranchId == branchId
                                && d.IPDate >= Convert.ToDateTime(startDate)
                                && d.IPDate <= Convert.ToDateTime(endDate)
                                select new Entities.TrnArticlePrice
                                {
                                    Id = d.Id,
                                    IPNumber = d.IPNumber,
                                    IPDate = d.IPDate.ToShortDateString(),
                                    ManualIPNumber = d.ManualIPNumber,
                                    Particulars = d.Particulars,
                                    PreparedById = d.PreparedById,
                                    CheckedById = d.CheckedById,
                                    ApprovedById = d.ApprovedById,
                                    IsLocked = d.IsLocked,
                                    CreatedById = d.CreatedById,
                                    CreatedBy = d.MstUser3.FullName,
                                    CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                    UpdatedById = d.UpdatedById,
                                    UpdatedBy = d.MstUser4.FullName,
                                    UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                                };

            return articlePrices.ToList();
        }

        // ====================
        // Detail Article Price
        // ====================
        [Authorize, HttpGet, Route("api/articlePrice/detail/{id}")]
        public Entities.TrnArticlePrice DetailArticlePrice(String id)
        {
            var currentUser = from d in db.MstUsers
                              where d.UserId == User.Identity.GetUserId()
                              select d;

            var branchId = currentUser.FirstOrDefault().BranchId;

            var articlePrice = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                               where d.BranchId == branchId
                               && d.Id == Convert.ToInt32(id)
                               select new Entities.TrnArticlePrice
                               {
                                   Id = d.Id,
                                   BranchId = d.BranchId,
                                   IPNumber = d.IPNumber,
                                   IPDate = d.IPDate.ToShortDateString(),
                                   ManualIPNumber = d.ManualIPNumber,
                                   Particulars = d.Particulars,
                                   PreparedById = d.PreparedById,
                                   CheckedById = d.CheckedById,
                                   ApprovedById = d.ApprovedById,
                                   IsLocked = d.IsLocked,
                                   CreatedById = d.CreatedById,
                                   CreatedBy = d.MstUser3.FullName,
                                   CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                   UpdatedById = d.UpdatedById,
                                   UpdatedBy = d.MstUser4.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                               };

            if (articlePrice.Any())
            {
                return articlePrice.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        // ==============================
        // Dropdown List - Branch (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/articlePrice/dropdown/list/branch")]
        public List<Entities.MstBranch> DropdownListArticlePriceBranch()
        {
            var branches = from d in db.MstBranches.OrderBy(d => d.Branch)
                           select new Entities.MstBranch
                           {
                               Id = d.Id,
                               Branch = d.Branch
                           };

            return branches.ToList();
        }

        // ============================
        // Dropdown List - User (Field)
        // ============================
        [Authorize, HttpGet, Route("api/articlePrice/dropdown/list/users")]
        public List<Entities.MstUser> DropdownListArticlePriceUsers()
        {
            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                        where d.IsLocked == true
                        select new Entities.MstUser
                        {
                            Id = d.Id,
                            FullName = d.FullName
                        };

            return users.ToList();
        }

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // =================
        // Add Article Price
        // =================
        [Authorize, HttpPost, Route("api/articlePrice/add")]
        public HttpResponseMessage AddArticlePrice()
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
                                    && d.SysForm.FormName.Equals("ItemPriceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanAdd)
                        {
                            var defaultIPNumber = "0000000001";
                            var lastArticlePrice = from d in db.TrnArticlePrices.OrderByDescending(d => d.Id)
                                                   where d.BranchId == currentBranchId
                                                   select d;

                            if (lastArticlePrice.Any())
                            {
                                var IPNumber = Convert.ToInt32(lastArticlePrice.FirstOrDefault().IPNumber) + 0000000001;
                                defaultIPNumber = FillLeadingZeroes(IPNumber, 10);
                            }

                            var users = from d in db.MstUsers.OrderBy(d => d.FullName)
                                        where d.IsLocked == true
                                        select d;

                            if (users.Any())
                            {
                                Data.TrnArticlePrice newArticlePrice = new Data.TrnArticlePrice
                                {
                                    BranchId = currentBranchId,
                                    IPNumber = defaultIPNumber,
                                    IPDate = DateTime.Today,
                                    ManualIPNumber = "NA",
                                    Particulars = "NA",
                                    PreparedById = currentUserId,
                                    CheckedById = currentUserId,
                                    ApprovedById = currentUserId,
                                    IsLocked = false,
                                    CreatedById = currentUserId,
                                    CreatedDateTime = DateTime.Now,
                                    UpdatedById = currentUserId,
                                    UpdatedDateTime = DateTime.Now
                                };

                                db.TrnArticlePrices.InsertOnSubmit(newArticlePrice);
                                db.SubmitChanges();

                                return Request.CreateResponse(HttpStatusCode.OK, newArticlePrice.Id);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No user found. Please setup more users for all transactions.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to add item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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

        // ==================
        // Lock Article Price
        // ==================
        [Authorize, HttpPut, Route("api/articlePrice/lock/{id}")]
        public HttpResponseMessage LockArticlePrice(Entities.TrnArticlePrice objArticlePrice, String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanLock)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (!articlePrice.FirstOrDefault().IsLocked)
                                {
                                    var lockArticlePrice = articlePrice.FirstOrDefault();
                                    lockArticlePrice.IPDate = Convert.ToDateTime(objArticlePrice.IPDate);
                                    lockArticlePrice.ManualIPNumber = objArticlePrice.ManualIPNumber;
                                    lockArticlePrice.Particulars = objArticlePrice.Particulars;
                                    lockArticlePrice.CheckedById = objArticlePrice.CheckedById;
                                    lockArticlePrice.ApprovedById = objArticlePrice.ApprovedById;
                                    lockArticlePrice.IsLocked = true;
                                    lockArticlePrice.UpdatedById = currentUserId;
                                    lockArticlePrice.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These item price details are already locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to lock item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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

        // ====================
        // Unlock Article Price
        // ====================
        [Authorize, HttpPut, Route("api/articlePrice/unlock/{id}")]
        public HttpResponseMessage UnlockArticlePrice(String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceDetail")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanUnlock)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (articlePrice.FirstOrDefault().IsLocked)
                                {
                                    var unlockArticlePrice = articlePrice.FirstOrDefault();
                                    unlockArticlePrice.IsLocked = false;
                                    unlockArticlePrice.UpdatedById = currentUserId;
                                    unlockArticlePrice.UpdatedDateTime = DateTime.Now;

                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These item price details are already unlocked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to unlock item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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

        // =================
        // Delete Article Price
        // =================
        [Authorize, HttpDelete, Route("api/articlePrice/delete/{id}")]
        public HttpResponseMessage DeleteArticlePrice(String id)
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
                                    && d.SysForm.FormName.Equals("ItemPriceList")
                                    select d;

                    if (userForms.Any())
                    {
                        if (userForms.FirstOrDefault().CanDelete)
                        {
                            var articlePrice = from d in db.TrnArticlePrices
                                               where d.Id == Convert.ToInt32(id)
                                               select d;

                            if (articlePrice.Any())
                            {
                                if (!articlePrice.FirstOrDefault().IsLocked)
                                {
                                    db.TrnArticlePrices.DeleteOnSubmit(articlePrice.First());
                                    db.SubmitChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete item price if the current item price record is locked.");
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item price details are not found in the server.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no rights to delete item price.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Sorry. You have no access for this item price page.");
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
