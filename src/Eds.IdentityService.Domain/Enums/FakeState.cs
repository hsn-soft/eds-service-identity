using System.ComponentModel.DataAnnotations;

namespace Eds.IdentityService.Domain.Enums;

public enum FakeState
{
    [Display(Name = "CreatedWaitForApprove")]
    WaitForApprove = 0,

    [Display(Name = "ApprovedWaitForSend")]
    ApprovedWaitForSend = 1,

    [Display(Name = "SentWaitForResult")]
    SentWaitForResult = 2,

    [Display(Name = "Fail")]
    ResultFail = 51,

    [Display(Name = "Success")]
    ResultSuccess = 99
}