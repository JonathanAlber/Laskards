namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Represents the team affiliation of a card.
    /// Is used to ensure the card collections only accept cards for the correct team
    /// and in the card players themselves to avoid playing cards on the wrong team.
    /// </summary>
    public enum ECardTeamAffiliation : byte
    {
        Player = 0,
        Boss = 1,
        Both = 2
    }
}