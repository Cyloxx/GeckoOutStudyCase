using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Rules;
using GeckoOut.Core.Session;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class LevelSessionTests
    {
        // 5x5 board. Wall at (1,0). Red exit at (3,1). Green exit at (0,3).
        // Red gecko: head (1,1), tail (1,2). Green gecko: (3,3)-(3,4).

        private LevelSession _session;
        private GeckoBody _red;
        private GeckoBody _green;

        [SetUp]
        public void SetUp()
        {
            var board = new BoardGrid(5, 5,
                new List<GridPosition> { new GridPosition(1, 0) },
                new List<ExitPoint>
                {
                    new ExitPoint(new GridPosition(3, 1), ColorId.Red),
                    new ExitPoint(new GridPosition(0, 3), ColorId.Green)
                });

            _red = new GeckoBody(ColorId.Red, new List<GridPosition>
            {
                new GridPosition(1, 1),
                new GridPosition(1, 2)
            });

            _green = new GeckoBody(ColorId.Green, new List<GridPosition>
            {
                new GridPosition(3, 3),
                new GridPosition(3, 4)
            });

            _session = new LevelSession(board,
                new List<GeckoBody> { _red, _green },
                new MoveValidator(board, new ColorMatchExitRule()),
                new PathResolver(),
                60f);
        }

        [Test]
        public void TryStepTo_ValidStep_MovesGeckoAndFiresEvent()
        {
            GeckoBody steppedGecko = null;
            _session.GeckoStepped += delegate(GeckoBody gecko) { steppedGecko = gecko; };

            bool moved = _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(2, 1));

            Assert.That(moved, Is.True);
            Assert.That(_red.Head, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(steppedGecko, Is.SameAs(_red));
        }

        [Test]
        public void TryStepTo_IntoWall_ReturnsFalse()
        {
            bool moved = _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(1, 0));

            Assert.That(moved, Is.False);
            Assert.That(_red.Head, Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void TryStepTo_BackToPreviousCell_UndoesTheStep()
        {
            var cellsBefore = new List<GridPosition>(_red.Cells);
            _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(2, 1));

            bool undone = _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(1, 1));

            Assert.That(undone, Is.True);
            Assert.That(_red.Cells, Is.EqualTo(cellsBefore));
        }
    
        [Test]
        public void TryStepTo_MatchingExit_RemovesGeckoAndFiresExited()
        {
            GeckoBody exitedGecko = null;
            _session.GeckoExited += delegate(GeckoBody gecko, ExitPoint exit) { exitedGecko = gecko; };

            _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(2, 1));
            _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(3, 1));

            Assert.That(exitedGecko, Is.SameAs(_red));
            Assert.That(_session.ActiveGeckos, Has.No.Member(_red));
            Assert.That(_session.State, Is.EqualTo(SessionState.Playing));
        }

        [Test]
        public void LastGeckoExited_WinsTheLevel()
        {
            bool wonFired = false;
            _session.LevelWon += delegate { wonFired = true; };

            _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(2, 1));
            _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(3, 1));

            _session.TryStepTo(_green, GeckoEnd.Head, new GridPosition(2, 3));
            _session.TryStepTo(_green, GeckoEnd.Head, new GridPosition(1, 3));
            _session.TryStepTo(_green, GeckoEnd.Head, new GridPosition(0, 3));

            Assert.That(_session.State, Is.EqualTo(SessionState.Won));
            Assert.That(wonFired, Is.True);
        }

        [Test]
        public void Tick_TimeRunsOut_LosesTheLevel()
        {
            bool lostFired = false;
            _session.LevelLost += delegate { lostFired = true; };

            _session.Tick(30f);
            Assert.That(_session.State, Is.EqualTo(SessionState.Playing));

            _session.Tick(31f);
            Assert.That(_session.State, Is.EqualTo(SessionState.Lost));
            Assert.That(lostFired, Is.True);
            Assert.That(_session.RemainingSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void TryStepTo_AfterLoss_ReturnsFalse()
        {
            _session.Tick(61f);

            bool moved = _session.TryStepTo(_red, GeckoEnd.Head, new GridPosition(2, 1));

            Assert.That(moved, Is.False);
        }

        [Test]
        public void TryDragTo_DistantFreeCell_WalksTheWholePath()
        {
            bool moved = _session.TryDragTo(_red, GeckoEnd.Head, new GridPosition(3, 2));

            Assert.That(moved, Is.True);
            Assert.That(_red.Head, Is.EqualTo(new GridPosition(3, 2)));
        }
    }
}