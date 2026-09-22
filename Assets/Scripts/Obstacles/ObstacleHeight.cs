using UnityEngine;

namespace CheeseDash
{
    /// <summary>
    /// The two obstacle sizes, taken from the two sprites cut out of the condiments sheet.
    ///
    ///   Short -> the narrow bottle, rotated to lie horizontally. About 0.45 units tall.
    ///   High  -> the widest bottle, standing upright. About 1.07 units tall.
    ///
    /// The difference is the height the player has to clear, so Short is a small hop and
    /// High needs a full-height jump.
    /// </summary>
    public enum ObstacleHeight
    {
        Short = 0,
        High = 1
    }

    /// <summary>
    /// Where an obstacle sits relative to the ground. This is deliberately a separate axis
    /// from <see cref="ObstacleHeight"/>:
    ///
    ///   Grounded -> sits ON the ground, so it must be JUMPED.
    ///   Overhead -> hangs ABOVE the ground leaving a gap at foot level, so it must be
    ///               CROUCHED under. This is how you give the crouch key (C) a purpose:
    ///               set an obstacle's placement to Overhead and tune its `clearance`.
    /// </summary>
    public enum ObstaclePlacement
    {
        Grounded = 0,
        Overhead = 1
    }

    /// <summary>The single input the player must perform to survive an obstacle.</summary>
    public enum DodgeAction
    {
        Jump = 0,
        Crouch = 1
    }

    public static class ObstacleHeightExtensions
    {
        /// <summary>Which action gets the player past an obstacle in this position.</summary>
        public static DodgeAction RequiredAction(this ObstaclePlacement placement)
        {
            return placement == ObstaclePlacement.Overhead ? DodgeAction.Crouch : DodgeAction.Jump;
        }

        /// <summary>Human-readable name, used in warnings and gizmo labels.</summary>
        public static string DisplayName(this ObstacleHeight height)
        {
            return height == ObstacleHeight.High ? "High" : "Short";
        }

        /// <summary>Human-readable name, used in warnings and gizmo labels.</summary>
        public static string DisplayName(this ObstaclePlacement placement)
        {
            return placement == ObstaclePlacement.Overhead ? "Overhead" : "Grounded";
        }
    }
}
