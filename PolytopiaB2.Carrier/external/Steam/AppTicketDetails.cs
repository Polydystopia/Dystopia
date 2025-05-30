using System.Net;
using SteamKit2;

namespace SteamTicketDecrypt.Console;

public class AppTicketDetails
{
    public byte[] AuthTicket { get; set; }
    public string GcToken { get; set; }
    public DateTime TokenGenerated { get; set; }
    public IPAddress SessionExternalIP { get; set; }
    public uint ClientConnectionTime { get; set; }
    public uint ClientConnectionCount { get; set; }
    public uint Version { get; set; }
    public SteamID SteamID { get; set; }
    public uint AppID { get; set; }
    public IPAddress OwnershipTicketExternalIP { get; set; }
    public IPAddress OwnershipTicketInternalIP { get; set; }
    public uint OwnershipFlags { get; set; }
    public DateTime OwnershipTicketGenerated { get; set; }
    public DateTime OwnershipTicketExpires { get; set; }
    public List<uint> Licenses { get; set; }
    public List<DlcDetails> Dlc { get; set; }
    public byte[] Signature { get; set; }
    public bool IsExpired { get; set; }
    public bool HasValidSignature { get; set; }
    public bool IsValid { get; set; }
}