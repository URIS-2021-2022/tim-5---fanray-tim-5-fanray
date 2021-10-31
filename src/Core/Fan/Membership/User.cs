﻿using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fan.Membership
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// When the user was created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The friendly name to display on posts.
        /// </summary>
        [StringLength(maximumLength: 256)]
        public string DisplayName { get; set; }
    }
}
