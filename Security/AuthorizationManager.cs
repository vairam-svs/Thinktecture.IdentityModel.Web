﻿using Microsoft.IdentityModel.Claims;

namespace Thinktecture.Samples
{
    public class AuthorizationManager : ClaimsAuthorizationManager
    {
        public override bool CheckAccess(AuthorizationContext context)
        {
            return base.CheckAccess(context);
        }
    }
}