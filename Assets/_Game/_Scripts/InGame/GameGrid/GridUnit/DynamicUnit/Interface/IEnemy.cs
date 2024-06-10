namespace _Game.GameGrid.Unit.DynamicUnit.Interface
{
    public interface IEnemy : ICharacter
    {
        public void AddToLevelManager();
        public void RemoveFromLevelManager();

        bool IsActive { get; }
    }
}
