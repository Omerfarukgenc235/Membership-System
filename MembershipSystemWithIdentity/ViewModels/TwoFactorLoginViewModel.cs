﻿using MembershipSystemWithIdentity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipSystemWithIdentity.ViewModels
{
    public class TwoFactorLoginViewModel
    {
        [Display(Name = "Doğrulama Kodunuz")]
        [Required(ErrorMessage = "Doğrulama kodu boş olamaz")]
        [StringLength(8, ErrorMessage = "Doğrulama kodunuz en fazla 8 haneli olabilir")]
        public string VerificationCode { get; set; }

        public bool isRememberMe { get; set; }
        public bool isRecoverCode { get; set; }
        public TwoFactor TwoFactorType { get; set; }
    }
}
