using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using MvcMembership;
using MvcMembership.Settings;
using EvolucionaMovil.Areas.MvcMembership.Models.Users;
using EvolucionaMovil.Models.Helpers;
using System.Text;
using EvolucionaMovil.Models;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{    
    public class UsersController : Controller
    {
        private const int PageSize = 10;
        private const string ResetPasswordBody = "Su nuevo password es: ";
        private const string ResetPasswordSubject = "Evoluciona Móvil - Nuevo password";
        private const string PasswordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private readonly IRolesService _rolesService;
        private readonly ISmtpClient _smtpClient;
        private readonly IMembershipSettings _membershipSettings;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;

        public UsersController()
            : this(new AspNetMembershipProviderWrapper(), new AspNetRoleProviderWrapper(), new SmtpClientProxy())
        {
        }

        public UsersController(AspNetMembershipProviderWrapper membership, IRolesService roles, ISmtpClient smtp)
            : this(membership.Settings, membership, membership, roles, smtp)
        {
        }

        public UsersController(
            IMembershipSettings membershipSettings,
            IUserService userService,
            IPasswordService passwordService,
            IRolesService rolesService,
            ISmtpClient smtpClient)
        {
            _membershipSettings = membershipSettings;
            _userService = userService;
            _passwordService = passwordService;
            _rolesService = rolesService;
            _smtpClient = smtpClient;
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Index(int? page, string search)
        {
            var users = string.IsNullOrWhiteSpace(search)
                ? _userService.FindAll(page ?? 1, PageSize)
                : search.Contains("@")
                    ? _userService.FindByEmail(search, page ?? 1, PageSize)
                    : _userService.FindByUserName(search, page ?? 1, PageSize);

            if (!string.IsNullOrWhiteSpace(search) && users.Count == 1)
                return RedirectToAction("Details", new { id = users[0].ProviderUserKey.ToString() });

            return View(new IndexViewModel
                            {
                                Search = search,
                                Users = users,
                                Roles = _rolesService.Enabled
                                    ? _rolesService.FindAll()
                                    : Enumerable.Empty<string>(),
                                IsRolesEnabled = _rolesService.Enabled
                            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult CreateRole(string id)
        {
            if (_rolesService.Enabled)
                _rolesService.Create(id);
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult DeleteRole(string id)
        {
            _rolesService.Delete(id);
            return RedirectToAction("Index");
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Role(string id)
        {
            return View(new RoleViewModel
                            {
                                Role = id,
                                Users = _rolesService.FindUserNamesByRole(id)
                                                     .ToDictionary(
                                                        k => k,
                                                        v => _userService.Get(v)
                                                     )
                            });
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Details(Guid id)
        {
            var user = _userService.Get(id);
            var userRoles = _rolesService.Enabled
                ? _rolesService.FindByUser(user)
                : Enumerable.Empty<string>();
            return View(new DetailsViewModel
                            {
                                CanResetPassword = _membershipSettings.Password.ResetOrRetrieval.CanReset,
                                RequirePasswordQuestionAnswerToResetPassword = _membershipSettings.Password.ResetOrRetrieval.RequiresQuestionAndAnswer,
                                DisplayName = user.UserName,
                                User = user,
                                Roles = _rolesService.Enabled
                                    ? _rolesService.FindAll().ToDictionary(role => role, role => userRoles.Contains(role))
                                    : new Dictionary<string, bool>(0),
                                IsRolesEnabled = _rolesService.Enabled,
                                Status = user.IsOnline
                                            ? DetailsViewModel.StatusEnum.Online
                                            : !user.IsApproved
                                                ? DetailsViewModel.StatusEnum.Unapproved
                                                : user.IsLockedOut
                                                    ? DetailsViewModel.StatusEnum.LockedOut
                                                    : DetailsViewModel.StatusEnum.Offline
                            });
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Password(Guid id)
        {
            var user = _userService.Get(id);
            var userRoles = _rolesService.Enabled
                ? _rolesService.FindByUser(user)
                : Enumerable.Empty<string>();
            return View(new DetailsViewModel
            {
                CanResetPassword = _membershipSettings.Password.ResetOrRetrieval.CanReset,
                RequirePasswordQuestionAnswerToResetPassword = _membershipSettings.Password.ResetOrRetrieval.RequiresQuestionAndAnswer,
                DisplayName = user.UserName,
                User = user,
                Roles = _rolesService.Enabled
                    ? _rolesService.FindAll().ToDictionary(role => role, role => userRoles.Contains(role))
                    : new Dictionary<string, bool>(0),
                IsRolesEnabled = _rolesService.Enabled,
                Status = user.IsOnline
                            ? DetailsViewModel.StatusEnum.Online
                            : !user.IsApproved
                                ? DetailsViewModel.StatusEnum.Unapproved
                                : user.IsLockedOut
                                    ? DetailsViewModel.StatusEnum.LockedOut
                                    : DetailsViewModel.StatusEnum.Offline
            });
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult UsersRoles(Guid id)
        {
            var user = _userService.Get(id);
            var userRoles = _rolesService.FindByUser(user);
            return View(new DetailsViewModel
            {
                CanResetPassword = _membershipSettings.Password.ResetOrRetrieval.CanReset,
                RequirePasswordQuestionAnswerToResetPassword = _membershipSettings.Password.ResetOrRetrieval.RequiresQuestionAndAnswer,
                DisplayName = user.UserName,
                User = user,
                Roles = _rolesService.FindAll().ToDictionary(role => role, role => userRoles.Contains(role)),
                IsRolesEnabled = true,
                Status = user.IsOnline
                            ? DetailsViewModel.StatusEnum.Online
                            : !user.IsApproved
                                ? DetailsViewModel.StatusEnum.Unapproved
                                : user.IsLockedOut
                                    ? DetailsViewModel.StatusEnum.LockedOut
                                    : DetailsViewModel.StatusEnum.Offline
            });
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult CreateUser()
        {
            var model = new CreateUserViewModel
                            {
                                InitialRoles = _rolesService.FindAll().ToDictionary(k => k, v => false)
                            };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult CreateUser(CreateUserViewModel createUserViewModel)
        {
            if (!ModelState.IsValid)
                return View(createUserViewModel);

            try
            {
                if (createUserViewModel.Password != createUserViewModel.ConfirmPassword)
                    throw new MembershipCreateUserException("Passwords do not match.");

                var user = _userService.Create(
                    createUserViewModel.Username,
                    createUserViewModel.Password,
                    createUserViewModel.Email,
                    createUserViewModel.PasswordQuestion,
                    createUserViewModel.PasswordAnswer,
                    true);

                if (createUserViewModel.InitialRoles != null)
                {
                    var rolesToAddUserTo = createUserViewModel.InitialRoles.Where(x => x.Value).Select(x => x.Key);
                    foreach (var role in rolesToAddUserTo)
                        _rolesService.AddToRole(user, role);
                }

                return RedirectToAction("Details", new { id = user.ProviderUserKey });
            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View(createUserViewModel);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult Details(Guid id, string email, string comments)
        {
            var user = _userService.Get(id);
            user.Email = email;
            user.Comment = comments;
            _userService.Update(user);
            return RedirectToAction("Details", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult DeleteUser(Guid id)
        {
            var user = _userService.Get(id);

            if (_rolesService.Enabled)
                _rolesService.RemoveFromAllRoles(user);
            _userService.Delete(user);
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult ChangeApproval(Guid id, bool isApproved)
        {
            var user = _userService.Get(id);
            user.IsApproved = isApproved;
            _userService.Update(user);
            return RedirectToAction("Details", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult Unlock(Guid id)
        {
            _passwordService.Unlock(_userService.Get(id));
            return RedirectToAction("Details", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult ResetPassword(Guid id)
        {
            var user = _userService.Get(id);
            //var newPassword = _passwordService.ResetPassword(user);
            var newPassword = GetRandomPassword();
            _passwordService.ChangePassword(user, newPassword);

            var body = ResetPasswordBody + newPassword;
            var msg = new MailMessage();
            msg.To.Add(user.Email);
            msg.Subject = ResetPasswordSubject;
            msg.Body = body;
            //TODO: Hacer la inyección de dependencia correctamente. Por lo pronto sólo llamo al helper que envía el email.
            //_smtpClient.Send(msg);
            EmailHelper.Enviar(body, ResetPasswordSubject, user.Email);

            return RedirectToAction("Password", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult ResetPasswordWithAnswer(Guid id, string answer)
        {
            var user = _userService.Get(id);
            var newPassword = _passwordService.ResetPassword(user, answer);

            var body = ResetPasswordBody + newPassword;
            var msg = new MailMessage();
            msg.To.Add(user.Email);
            msg.Subject = ResetPasswordSubject;
            msg.Body = body;
            //TODO: Hacer la inyección de dependencia correctamente. Por lo pronto sólo llamo al helper que envía el email.
            //_smtpClient.Send(msg);
            EmailHelper.Enviar(body, ResetPasswordSubject, user.Email);

            return RedirectToAction("Password", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult SetPassword(Guid id, string password)
        {
            var user = _userService.Get(id);
            _passwordService.ChangePassword(user, password);

            var body = ResetPasswordBody + password;
            var msg = new MailMessage();
            msg.To.Add(user.Email);
            msg.Subject = ResetPasswordSubject;
            msg.Body = body;
            //TODO: Hacer la inyección de dependencia correctamente. Por lo pronto sólo llamo al helper que envía el email.
            //_smtpClient.Send(msg);
            EmailHelper.Enviar(body, ResetPasswordSubject, user.Email);

            return RedirectToAction("Password", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult AddToRole(Guid id, string role)
        {
            _rolesService.AddToRole(_userService.Get(id), role);
            return RedirectToAction("UsersRoles", new { id });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public RedirectToRouteResult RemoveFromRole(Guid id, string role)
        {
            _rolesService.RemoveFromRole(_userService.Get(id), role);
            return RedirectToAction("UsersRoles", new { id });
        }

        [HttpGet]
        public ActionResult PasswordRecovery()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RecoveryResult(bool succeed)
        {
            ViewBag.Succeed = succeed;
            return View();
        }

        [HttpPost]
        public RedirectToRouteResult PasswordRecovery(RecoveryVM recoveryVM)
        {
            MembershipUser user = null;
            var succeed = false;
            if (!string.IsNullOrWhiteSpace(recoveryVM.UserName))
            {
                user = _userService.Get(recoveryVM.UserName);
            }
            else if (!string.IsNullOrWhiteSpace(recoveryVM.Email))
            {
                user = _userService.FindByEmail(recoveryVM.Email, 1, 1).FirstOrDefault();
            }
            if (user == null)
            {

                return RedirectToAction("RecoveryResult", new { succeed });
            }
            //El usuario si existe. Se genera el nuevo password y se manda el email
            var password = GetRandomPassword();
            _passwordService.ChangePassword(user, password);

            var body = ResetPasswordBody + password;
            var msg = new MailMessage();

            EmailHelper.Enviar(body, ResetPasswordSubject, user.Email);
            succeed = true;
            return RedirectToAction("RecoveryResult", new { succeed });
        }

        private string GetRandomPassword()
        {
            StringBuilder newPassword = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                newPassword.Append(PasswordCharacters.ToArray()[new Random(Guid.NewGuid().GetHashCode()).Next(0, PasswordCharacters.ToArray().Length - 1)]);
            }
            return newPassword.ToString();
        }
    }
}
