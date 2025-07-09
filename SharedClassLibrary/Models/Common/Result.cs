namespace SharedClassLibrary.Models.Common;


public class Error
{
    public ErrorCode Code { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum ErrorCode
{
    None = 0,
    ValidationError = 1,
    VerificationCodeNotValid = 2,
    NotFound = 3,
    FileRequired = 4,
    FileSizeExceedsLimit = 5,
    FileUploadFailed = 6,
    InvalidImageFile = 7,
    FileDeleteFailed = 8,
    InvalidCredentials = 1000,
    EmailNotConfirmed = 1001,
    UserNotFound = 1002,
    EmailAlreadyExists = 1003,
    UsernameAlreadyExists = 1004,
    RegistrationFailed = 1005,
    InvalidToken = 1006,
    TokenCreationFailed = 1007,
    UnregisteredEmail = 1008,
    ExternalLoginFailed = 1009,
    ExternalLoginEmailRequired = 1010,
    UnsupportedExternalProvider = 1011,
    FailedToUpdateData = 1012,
    HandTalkServiceUnavailable = 2000,
    HandTalkGlossNotFound = 2001,
    HandTalkProcessingError = 2002,
    HandTalkInvalidInput = 2003,
    HandTalkRateLimitExceeded = 2004,
    HandTalkEmptyResponse = 2005,
    DictionaryEntryAlreadyExists = 3000,
    DictionaryEntryNotExists = 3001,
    MaximumFavoritesReached = 3002
}

public class Result
{
    public bool IsSuccess { get; set; }
    public Error Error { get; set; } = new Error();
}

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public Error Error { get; set; } = new Error();
    public T? Data { get; set; }
}
