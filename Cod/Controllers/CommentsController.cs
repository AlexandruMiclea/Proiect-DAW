﻿using Cod.Data;
using Cod.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cod.Controllers
{
    public class CommentsController : Controller
    {
        // manager de useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> um;
        private readonly RoleManager<IdentityRole> rm;
        public CommentsController(ApplicationDbContext _db, UserManager<ApplicationUser> _um, RoleManager<IdentityRole> _rm)
        {
            db = _db;
            um = _um;
            rm = _rm;
        }

        // TODO debug method

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var comments = db.Comments.Include("Profile").OrderBy(x => x.CreationDate);
            ViewBag.Comments = comments;

            return View();
        }

        [HttpGet]
        public IActionResult Show(int id)
        {
            Comment comment = db.Comments.Include("Profile").Where(x => x.Id == id).First();
            return View(comment);
        }

        // TODO link comment with post

        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            return View(comment);
        }

        [HttpGet]
        public IActionResult New(int id)
        {
            Comment comment = new Comment();
            return View(comment);
        }

        [HttpPost]
        public IActionResult New(Comment comment)
        {
            // TODO validate ModelState.IsValid
            // brute force values
            comment.ProfileId = um.GetUserId(User);
            comment.CreationDate = DateTime.Now;
            // TODO hardcodat
            comment.PostId = 2;
            db.SaveChanges();

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Index");
            } else
            {
                return View(comment);
            }
        }

        // TODO check if user that made the post is the user trying to modify the post

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comment = db.Comments.Where(x => x.Id == id).First();

            if (comment.ProfileId == um.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comment);
            }
            else
            {
                TempData["message"] = "Nu puteti modifica acest comentariu!";
                TempData["messageType"] = "alert-danger";
                // TODO where redirect?
                return RedirectToAction("/Posts/Show/" + comment.PostId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Comment reqComment)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.ProfileId == um.GetUserId(User) || User.IsInRole("Admin"))
            {
                comment.Content = reqComment.Content;
                db.SaveChanges();
                TempData["message"] = "Comentariul a fost modificat!";
                TempData["messageType"] = "alert-success";
                // TODO where redirect
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu puteti modifica acest comentariu!";
                TempData["messageType"] = "alert-danger";
                // TODO where redirect?
                return RedirectToAction("/Posts/Show/" + comment.PostId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comment = db.Comments.Where(x => x.Id == id).First();
            if (comment.ProfileId == um.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                TempData["message"] = "Comentariul a fost sters!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("/Posts/Show/" + comment.PostId);
            } else
            {
                TempData["message"] = "Nu puteti sterge acest comentariu!";
                TempData["messageType"] = "alert-danger";
                // TODO where redirect?
                return RedirectToAction("/Posts/Show/" + comment.PostId);
            }
        }
    }
}
