using PolytopiaBackendBase;

namespace Dystopia.Models.Util;

public enum DystopiaErrorCode
{
    None = 0,
    Success = 1,
    ConnectionError = 404, // 0x00000194
    InternalServerError = 500, // 0x000001F4
    SubmitFailedBusy = 1001, // 0x000003E9
    SteamAuthenticationFailed = 1002, // 0x000003EA
    GooglePlayAuthenticationFailed = 1003, // 0x000003EB
    LoginFailed = 1004, // 0x000003EC
    GameNotFound = 1005, // 0x000003ED
    PlayerNotFound = 1006, // 0x000003EE
    InvalidFriendRequest = 1007, // 0x000003EF
    FriendshipNotFound = 1008, // 0x000003F0
    InvalidFriendshipStatus = 1009, // 0x000003F1
    GameCreationNoPlayers = 1010, // 0x000003F2
    NoGameStateData = 1011, // 0x000003F3
    GameStateDeserializationFailed = 1012, // 0x000003F4
    InvalidUserCommand = 1013, // 0x000003F5
    KickPlayerFailed = 1014, // 0x000003F6
    StartGameFailed = 1015, // 0x000003F7
    UserNotFound = 1016, // 0x000003F8
    PickTribeFailed = 1017, // 0x000003F9
    AICommandFailed = 1018, // 0x000003FA
    UnsupportedCreateVersion = 1019, // 0x000003FB
    RemindPlayerFailed = 1020, // 0x000003FC
    StateProhibitsOperation = 1021, // 0x000003FD
    EntityWorkerHandleFailed = 1022, // 0x000003FE
    UnsupportedOpenVersion = 1023, // 0x000003FF
    ReplayGameStillOngoing = 1024, // 0x00000400
    FavouriteLimitReached = 1025, // 0x00000401
    MissingData = 1026, // 0x00000402
    AdminRegisterFailed = 1027, // 0x00000403
    MissingLobby = 1028, // 0x00000404
    GameCreationTooManyPlayers = 1029, // 0x00000405
    GameVersionTooOld = 1030, // 0x00000406
    FriendRequestNotAllowed = 1031, // 0x00000407
    CmUnknown = 10000, // 0x00002710
    CmParseIntent = 10001, // 0x00002711
    CmReportGameFailed = 10003, // 0x00002713
    CmGameAccountAlreadyLinked = 10004, // 0x00002714
    CmRefreshTokenExpired = 10005, // 0x00002715
    CmUserAuthenticationFailed = 10006, // 0x00002716
    CmUserAccountAlreadyUsed = 10007, // 0x00002717
    CmGameAccountNotLinked = 10008, // 0x00002718
    CmTriedToResendAuthCode = 10009, // 0x00002719
    CmNotPossibleInGame = 10010, // 0x0000271A
    CmActionNotAllowed = 10011, // 0x0000271B
    CmStateProhibitsOperation = 10012, // 0x0000271C
    CmTechnicalError = 10013, // 0x0000271D
    CmNotEnabled = 10014, // 0x0000271E
    LoginFailedAlreadyLoggingIn = 20000, // 0x00004E20
    LoginFailedUserHasNotGivenServerConsent = 20001, // 0x00004E21
    LoginFailedServerAccessNotSupported = 20002, // 0x00004E22
    LoginFailedException = 20003, // 0x00004E23
    LoginFailedTeslaInitFailed = 20010, // 0x00004E2A
    LoginFailedTeslaEnvironmentUnknown = 20011, // 0x00004E2B
    LoginFailedTeslaGamerException = 20012, // 0x00004E2C
    LoginFailedTeslaGamerMissing = 20013, // 0x00004E2D
    LoginFailedSteamTicketNotFound = 20020, // 0x00004E34
    LoginFailedAndroidUISignInRequired = 20030, // 0x00004E3E
    LoginFailedAndroidUserCanceled = 20031, // 0x00004E3F
    LoginFailedAndroidNotAuthenticated = 20032, // 0x00004E40
    LoginFailedAndroidEmptyAuthCode = 20033, // 0x00004E41
    LoginFailediOSGameCenterNotLoggedIn = 20040, // 0x00004E48
    LoginFailediOSFailedException = 20041, // 0x00004E49
}

public static class DystopiaErrorCodeExtensions
{
    public static ErrorCode Map(this DystopiaErrorCode code)
    {
        return (ErrorCode)code;
    }
}