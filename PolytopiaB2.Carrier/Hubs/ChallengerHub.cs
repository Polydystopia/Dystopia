using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub //TODO tournaments are not implemented yet. only a placeholder
{
    public async Task<ServerResponse<ChallengermodeConnectionStatus>> GetChallengermodeConnectionStatus()
    {
        var status = new ChallengermodeConnectionStatus();
        status.IsConnected = true;
        status.IsAnotherAccountConnected = false;
        status.IsRefreshtokenExpired = false;
        status.ChallengermodeUserId = _userGuid;

        return new ServerResponse<ChallengermodeConnectionStatus>(status);
    }

    public async Task<ServerResponse<TournamentListViewModel>> GetTournamentList(
        GetTournamentListBindingModel bindingModel)
    {
        var tournament = new TournamentViewModel();
        tournament.Id = Guid.Empty;
        tournament.Name = "Not implemented yet";
        tournament.Description = "Tournaments are not implemented yet. This might change in the future. :)";
        tournament.ContactUrl = "https://discord.gg/rtwgWTzxWy";
        tournament.CustomPrizeText = "Tournaments are not implemented yet. This might change in the future. :)";

        var gameSettings = new TournamentGameSettingsViewModel();
        gameSettings.MapWidth = 0;
        gameSettings.MapPreset = MapPreset.None;
        gameSettings.GameMode = GameMode.Custom;
        gameSettings.DisabledTribes = new List<int>();
        gameSettings.TimeLimit = 0;
        gameSettings.AllowSteamPlayers = true;
        gameSettings.AllowMobilePlayers = true;

        tournament.GameSettings = gameSettings;
        tournament.DateCreated = DateTime.Today;

        tournament.Members = new List<TournamentMemberViewModel>();

        tournament.OverviewUrl = "https://discord.gg/rtwgWTzxWy";
        tournament.JoinUrl = "https://discord.gg/rtwgWTzxWy";
        tournament.AvailableSlots = 0;
        tournament.TotalSlots = 0;
        tournament.NumberRegistered = 0;
        tournament.TournamentFormat = TournamentFormat.Unknown;
        tournament.State = TournamentState.Unknown;
        tournament.PlayersPerLineup = 0;
        tournament.HostName = "Polydystopia";
        tournament.HasVerifiedHost = true;
        tournament.OfficialTournament = true;
        tournament.LogoUrl = "https://avatars.githubusercontent.com/u/120461041";

        tournament.PersonalViewModel = TournamentPersonalViewModel.None;

        tournament.MinimumGameVersion = 1;

        var list = new TournamentListViewModel();

        list.Tournaments = new List<TournamentViewModel>();
        list.Tournaments.Add(tournament);

        return new ServerResponse<TournamentListViewModel>(list);
    }
}