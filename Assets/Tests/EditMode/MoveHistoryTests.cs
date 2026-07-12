using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Commands;
using GeckoOut.Core.Gecko;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class MoveHistoryTests
    {
        private GeckoBody _gecko;
        private MoveHistory _history;

        [SetUp]
        public void SetUp()
        {
            // Straight gecko: head (2,1), middle (2,2), tail (2,3).
            _gecko = new GeckoBody(ColorId.Red, new List<GridPosition>
            {
                new GridPosition(2, 1),
                new GridPosition(2, 2),
                new GridPosition(2, 3)
            });

            _history = new MoveHistory();
        }

        [Test]
        public void ExecuteAndRecord_MovesGeckoAndGrowsHistory()
        {
            var command = new StepCommand(_gecko, GeckoEnd.Head, new GridPosition(2, 0));

            _history.ExecuteAndRecord(command);

            Assert.That(_gecko.Head, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(_history.Count, Is.EqualTo(1));
        }

        [Test]
        public void UndoLast_RestoresExactPreviousState()
        {
            var cellsBefore = new List<GridPosition>(_gecko.Cells);

            _history.ExecuteAndRecord(
                new StepCommand(_gecko, GeckoEnd.Head, new GridPosition(2, 0)));
            _history.UndoLast();

            Assert.That(_gecko.Cells, Is.EqualTo(cellsBefore));
            Assert.That(_history.Count, Is.EqualTo(0));
        }

        [Test]
        public void UndoLast_AfterSeveralSteps_UndoesOnlyTheLastOne()
        {
            _history.ExecuteAndRecord(
                new StepCommand(_gecko, GeckoEnd.Head, new GridPosition(2, 0)));
            _history.ExecuteAndRecord(
                new StepCommand(_gecko, GeckoEnd.Head, new GridPosition(1, 0)));

            _history.UndoLast();

            Assert.That(_gecko.Head, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(_history.Count, Is.EqualTo(1));
        }

        [Test]
        public void UndoLast_EmptyHistory_DoesNothing()
        {
            Assert.DoesNotThrow(delegate
            {
                _history.UndoLast();
            });
        }

        [Test]
        public void UndoLast_TailMove_RestoresState()
        {
            var cellsBefore = new List<GridPosition>(_gecko.Cells);

            _history.ExecuteAndRecord(
                new StepCommand(_gecko, GeckoEnd.Tail, new GridPosition(2, 4)));
            _history.UndoLast();

            Assert.That(_gecko.Cells, Is.EqualTo(cellsBefore));
        }

        [Test]
        public void Execute_Twice_Throws()
        {
            var command = new StepCommand(_gecko, GeckoEnd.Head, new GridPosition(2, 0));
            command.Execute();

            Assert.Throws<System.InvalidOperationException>(delegate
            {
                command.Execute();
            });
        }
    }
}