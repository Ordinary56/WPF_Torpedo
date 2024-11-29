// TODO: write the tiles for each ship
namespace WPF_Torpedo.Models
{
    public abstract class Ship
    {
        /// <summary>
        /// Tiles for each ship.
        /// The array order is this: 
        /// <para>1. Starting position (0° rotation) </para>
        /// <para>2. 90° degrees clockwise </para>
        /// <para>3. 180° degrees clockwise </para>
        /// <para>4. 270° degrees clockwise </para>
        /// </summary>
        protected abstract Position[][] Tiles { get; }
        /// <summary>
        /// The Ship's name
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// The starting offset for the ship
        /// <para>Use this for offsetting the ship's position when placing it on the gamegrid</para>
        /// </summary>
        protected Position StartOffset { get; set; }
        /// <summary>
        /// Unique ID for the ship
        /// <para>use it for identifying the ship on the grid</para>
        /// <remarks>The ID order is first -> 5 spaces ship, last -> 2 spaces ship</remarks>
        /// </summary>
        public abstract sbyte ID { get; }
        int _rotationState;
        Position _offset;
        protected Ship()
        {
            _offset = new Position {X = StartOffset.X, Y = StartOffset.Y};
        }
        protected Ship(Position offset) : this()
        {
            StartOffset = offset;
        }
        /// <summary>
        /// Get's the current position ship of the ship
        /// </summary>
        /// <returns>The position in X,Y pair</returns>
        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[_rotationState]) 
            {
                yield return new Position { X = (sbyte)(p.X + _offset.X), Y = (sbyte)(p.Y + _offset.Y) };
            }
        }
        /// <summary>
        /// Rotates the ship clockwise
        /// </summary>
        public void RotateCW()
        {
            _rotationState = (_rotationState + 1) % Tiles.Length;
        }
        /// <summary>
        /// Rotates the ship counter-clockwise
        /// </summary>
        public void RotateCCW()
        {
            if(_rotationState == 0)
            {
                _rotationState = Tiles.Length - 1;
                return;
            }
            _rotationState--;
        }
        /// <summary>
        /// Moves the ship to a different place
        /// </summary>
        /// <param name="newOffset">The new position in [X,Y]</param>
        public void Move(Position newOffset)
        {
            _offset = newOffset;
        }
    }
    // 5 spaces
    public class Carrier : Ship
    {
        public override string Name => "Carrier";

        public override sbyte ID => 1;

        protected override Position[][] Tiles =>
        [
            [
                new Position {X = 0, Y = 0},
                new Position {X = 1, Y = 0},
                new Position {X = 2, Y = 0},
                new Position {X = 3, Y = 0},
                new Position {X = 4, Y = 0},
                new Position {X = 5, Y = 0}
            ],
            [
               new Position {},
               new Position {},
               new Position {},
               new Position {},
               new Position {},
            ]
        ];

    }
    // 4 spaces
    public class BattleShip : Ship
    {
        public override string Name => "BattleShip";
        public override sbyte ID => 2;

        protected override Position[][] Tiles => 
        [
            [
                new Position {X = 0, Y = 0},
                new Position {X = 1, Y = 0},
                new Position {X = 2, Y = 0},
                new Position {X = 3, Y = 0},
            ]

        ];

    }
    // 3 spaces
    public class Cruiser : Ship
    {
        public override string Name => "Cruiser";
        public override sbyte ID => 3;

        protected override Position[][] Tiles => 
        [
            [
                new Position {X = 0, Y = 0},
                new Position {X = 1, Y = 0},
                new Position {X = 2, Y = 0},
            ]
        ];

    }
    // 3 spaces
    public class Submarine : Ship
    {
        public override string Name => "Submarine";

        public override sbyte ID => 4;

        protected override Position[][] Tiles => 
        [
            [
                new Position {X = 0, Y = 0},
                new Position {X = 1, Y = 0},
                new Position {X = 2, Y = 0},
            ]
        ];

    }
    // 2 spaces
    public class Destroyer : Ship
    {
        public override string Name => "Destroyer";
        public override sbyte ID => 5;

        protected override Position[][] Tiles => 
        [
            [
                new Position {X = 0, Y=0},
                new Position {X = 1, Y=0},
            ]
        ];

    }
    
}
