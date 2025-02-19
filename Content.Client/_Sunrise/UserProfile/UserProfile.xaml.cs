using Content.Client._Sunrise.SponsorTiers;
using Content.Shared._Sunrise.SunriseCCVars;
using Content.Sunrise.Interfaces.Client;
using Content.Sunrise.Interfaces.Shared;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._Sunrise.UserProfile;

[GenerateTypedNameReferences]
public sealed partial class UserProfile : Control
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;


    private readonly SponsorTiersUIController _sponsorTiersUIController;
    private readonly IClientServiceAuthManager? _serviceAuthManager;
    private readonly ISharedSponsorsManager? _sponsorsManager;

    private string _donateUrl = string.Empty;

    public UserProfile()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        IoCManager.Instance!.TryResolveType(out _sponsorsManager);
        IoCManager.Instance!.TryResolveType(out _serviceAuthManager);

        _sponsorTiersUIController = UserInterfaceManager.GetUIController<SponsorTiersUIController>();

        _cfg.OnValueChanged(SunriseCCVars.InfoLinksDonate, OnInfoLinksDonateChanged, true);


        if (_serviceAuthManager == null)
        {
            LinkDiscordButton.Disabled = true;
            LinkTelegramButton.Disabled = true;
            LinkGitHubButton.Disabled = true;
        }
        else
        {
            LinkTelegramButton.OnPressed += LinkTelegramPressed;
            LinkDiscordButton.OnPressed += LinkDiscordPressed;
            LinkGitHubButton.OnPressed += LinkGitHubPressed;
            ResetTelegramButton.OnPressed += ResetTelegramPressed;
            ResetDiscordButton.OnPressed += ResetDiscordPressed;
            ResetGitHubButton.OnPressed += ResetGitHubPressed;
            _serviceAuthManager.LoadedServiceLinkedServices += RefreshServiceLinkedServices;
        }

        if (_sponsorsManager == null)
        {
            BuySponsorButton.Disabled = true;
            InfoSponsorButton.Disabled = true;
        }
        else
        {
            BuySponsorButton.OnPressed += BuySponsorPressed;
            _sponsorsManager.LoadedSponsorInfo += RefreshSponsorInfo;
            InfoSponsorButton.OnPressed += InfoSponsorPressed;
        }
    }

    private void OnInfoLinksDonateChanged(string url)
    {
        BuySponsorButton.Disabled = url == "";
        _donateUrl = url;
    }

    private void RefreshSponsorInfo()
    {
        if (_sponsorsManager == null)
            return;

        if (_playerManager.LocalSession != null)
        {
            if (_sponsorsManager.ClientIsSponsor())
            {
                _sponsorsManager.TryGetOocTitle(_playerManager.LocalSession.UserId, out var sponsorTitle);
                SponsorTierName.Text = sponsorTitle;
            }
            else
            {
                SponsorTierName.Text = Loc.GetString("user-profile-no-sponsor");
            }
        }
    }

    private void RefreshServiceLinkedServices(List<LinkedServiceData> linkedServices)
    {
        if (_serviceAuthManager == null)
            return;

        // Reset visibility and text for all services
        LinkDiscordButton.Visible = true;
        LinkedDiscordName.Text = string.Empty;
        ResetDiscordButton.Visible = false;

        LinkTelegramButton.Visible = true;
        LinkedTelegramName.Text = string.Empty;
        ResetTelegramButton.Visible = false;

        LinkGitHubButton.Visible = true;
        LinkedGitHubName.Text = string.Empty;
        ResetGitHubButton.Visible = false;

        // Update visibility and text based on linked services
        foreach (var serviceAuthData in linkedServices)
        {
            switch (serviceAuthData.ServiceType)
            {
                case ServiceType.Discord:
                    LinkDiscordButton.Visible = false;
                    LinkedDiscordName.Text = serviceAuthData.Username;
                    ResetDiscordButton.Visible = true;
                    break;
                case ServiceType.Telegram:
                    LinkTelegramButton.Visible = false;
                    LinkedTelegramName.Text = serviceAuthData.Username;
                    ResetTelegramButton.Visible = true;
                    break;
                case ServiceType.Github:
                    LinkGitHubButton.Visible = false;
                    LinkedGitHubName.Text = serviceAuthData.Username;
                    ResetGitHubButton.Visible = true;
                    break;
            }
        }
    }

    private void ResetTelegramPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ResetServiceLink(ServiceType.Telegram);
    }

    private void ResetDiscordPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ResetServiceLink(ServiceType.Discord);
    }

    private void ResetGitHubPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ResetServiceLink(ServiceType.Github);
    }

    private void BuySponsorPressed(BaseButton.ButtonEventArgs obj)
    {
        _uri.OpenUri(_donateUrl);
    }

    private void LinkTelegramPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ToggleWindow(ServiceType.Telegram);
    }

    private void LinkDiscordPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ToggleWindow(ServiceType.Discord);
    }

    private void LinkGitHubPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ToggleWindow(ServiceType.Github);
    }

    private void InfoSponsorPressed(BaseButton.ButtonEventArgs obj)
    {
        _sponsorTiersUIController.ToggleWindow();
    }
}
