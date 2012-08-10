namespace Kululu.Entities.Common
{
    /// <summary>
    /// possible user scores for various operations
    /// </summary>
    public enum ActionsScore
    {
        RemoveSong = -10,
        RemoveVote = -5,
        Vote = 5,
        AddSong = 10
    }
}
