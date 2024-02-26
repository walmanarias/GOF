using GOF.Entities;
using GOF.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GOF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
         private readonly GameContext _context;

        public GameController(GameContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateBoard([FromBody] bool[,] state)
        {
            var board = new Board { State = state };
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return board.Id;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<bool[,]>> GetNextState(int id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return NotFound();
            }

            board.State = ComputeNextState(board.State);
            await _context.SaveChangesAsync();

            return board.State;
        }

        [HttpGet("{id}/{generations}")]
        public async Task<ActionResult<bool[,]>> GetFutureState(int id, int generations)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return NotFound();
            }

            for (int i = 0; i < generations; i++)
            {
                board.State = ComputeNextState(board.State);
            }

            await _context.SaveChangesAsync();

            return board.State;
        }

        [HttpGet("{id}/final")]
        public async Task<ActionResult<bool[,]>> GetFinalState(int id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return NotFound();
            }

            const int maxAttempts = 1000;
            for (int i = 0; i < maxAttempts; i++)
            {
                var newState = ComputeNextState(board.State);
                if (newState == board.State)
                {
                    break;
                }

                board.State = newState;
            }

            if (board.State == ComputeNextState(board.State))
            {
                return board.State;
            }
            else
            {
                return Problem("The board did not reach a conclusion after the maximum number of attempts.");
            }
        }

        private bool[,] ComputeNextState(bool[,] currentState)
        {
            int rows = currentState.GetLength(0);
            int cols = currentState.GetLength(1);
            bool[,] nextState = new bool[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int aliveNeighbors = CountAliveNeighbors(currentState, i, j);
        
                    if (currentState[i, j])
                    {
                        nextState[i, j] = aliveNeighbors == 2 || aliveNeighbors == 3;
                    }
                    else
                    {
                        nextState[i, j] = aliveNeighbors == 3;
                    }
                }
            }

            return nextState;
        }

        private int CountAliveNeighbors(bool[,] grid, int row, int col)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < grid.GetLength(0) && newCol >= 0 && newCol < grid.GetLength(1))
                    {
                        if (grid[newRow, newCol])
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

    }
}
