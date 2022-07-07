using Microsoft.Xna.Framework;

namespace Sokoban;

interface ITimeShiftable
{
    public void ShiftTo(int time);
    public void NewTime(int time);
    public void ResetTime();
}