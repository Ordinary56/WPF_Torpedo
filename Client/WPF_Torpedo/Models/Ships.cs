using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public abstract string Name { get; }
        protected abstract Position StartOffset { get; }
        public abstract sbyte ID { get; }
        int _rotationState;
        Position _offset;
        public Ship()
        {
            _offset = new Position {X = StartOffset.X, Y = StartOffset.Y};
        }
        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[_rotationState]) 
            {
                yield return new Position { X = (sbyte)(p.X + _offset.X), Y = (sbyte)(p.Y + _offset.Y) };
            }
        }
        public void RotateCW()
        {
            _rotationState = (_rotationState + 1) % Tiles.Length;
        }
        public void RotateCCW()
        {
            if(_rotationState == 0)
            {
                _rotationState = Tiles.Length - 1;
                return;
            }
            _rotationState--;
        }
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

            ]
        ];

        protected override Position StartOffset => throw new NotImplementedException();
    }
    // 4 spaces
    public class BattleShip : Ship
    {
        public override string Name => "BattleShip";
        public override sbyte ID => 2;

        protected override Position[][] Tiles => throw new NotImplementedException();

        protected override Position StartOffset => throw new NotImplementedException();
    }
    // 3 spaces
    public class Cruiser : Ship
    {
        public override string Name => "Cruiser";
        public override sbyte ID => 3;

        protected override Position[][] Tiles => throw new NotImplementedException();

        protected override Position StartOffset => throw new NotImplementedException();
    }
    // 3 spaces
    public class Submarine : Ship
    {
        public override string Name => "Submarine";

        public override sbyte ID => 4;

        protected override Position[][] Tiles => throw new NotImplementedException();

        protected override Position StartOffset => throw new NotImplementedException();
    }
    // 2 spaces
    public class Destroyer : Ship
    {
        public override string Name => "Destroyer";
        public override sbyte ID => 5;

        protected override Position[][] Tiles => throw new NotImplementedException();

        protected override Position StartOffset => throw new NotImplementedException();
    }
    
}
