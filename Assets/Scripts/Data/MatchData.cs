using UnityEngine;

namespace ArrowWar.Data
{
    /// <summary>
    /// Defines one complete match: an ordered list of rounds.
    /// Player castle HP is NOT reset between rounds.
    /// Gold accumulates across all rounds and is spent after the match ends.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMatchData", menuName = "ArrowWar/Match Data")]
    public class MatchData : ScriptableObject
    {
        [Tooltip("Rounds played in order. Standard game = 3 rounds. " +
                 "Player castle HP is preserved across all rounds.")]
        public RoundData[] rounds = new RoundData[3];
    }
}
