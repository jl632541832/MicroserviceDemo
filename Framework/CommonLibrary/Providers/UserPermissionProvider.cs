﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public interface IUserPermissonProvider
    {
        Task<CurrentUserPermission> GetCurrentUserPermission();
    }

    public class UserPermissionProvider : IUserPermissonProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICallGeneralServiceApi callGeneralServiceApi;
        public UserPermissionProvider(IHttpContextAccessor _httpContextAccessor, ICallGeneralServiceApi _callGeneralServiceApi)
        {
            httpContextAccessor = _httpContextAccessor;
            callGeneralServiceApi = _callGeneralServiceApi;
        }

        public async Task<CurrentUserPermission> GetCurrentUserPermission()
        {
            if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new FriendlyException()
                {
                    ExceptionCode = 401,
                    ExceptionMessage = "Unauthorized Request."
                };
            }
            Claim subClaim = httpContextAccessor.HttpContext.User.FindFirst("sub");
            Claim auth_timeClaim = httpContextAccessor.HttpContext.User.FindFirst("auth_time");
            if (subClaim == null || auth_timeClaim == null)
            {
                throw new FriendlyException()
                {
                    ExceptionCode = 401,
                    ExceptionMessage = "The user claim: sub or auth_time is null."
                };
            }
            if (httpContextAccessor.HttpContext.Request.Path.HasValue && httpContextAccessor.HttpContext.Request.Path.Value.StartsWith("/api/userpermission/get/"))
            {
                return new CurrentUserPermission()
                {
                    RoleCode = "",
                    RoleName = "",
                    AllowResourceCodes = new List<string>() { "userpermission.get" }
                };
            }
            return await GetUserPermissonFromRedis(subClaim.Value, auth_timeClaim.Value); ;
        }

        private async Task<CurrentUserPermission> GetUserPermissonFromRedis(string subject, string auth_time)
        {
            //string redisKey = $"CurrentUserInfo_{subject}";
            string redisKey = $"CurrentUserPermission_{subject}_{auth_time}";
            var userPermission = await RedisHelper.GetAsync<CurrentUserPermission>(redisKey);
            if (userPermission == null)
            {
                //读取用户信息
                userPermission = await callGeneralServiceApi.GetUserPermission(subject);

                await RedisHelper.SetAsync(redisKey, userPermission, 36000);
            }
            return userPermission;
        }
    }



}