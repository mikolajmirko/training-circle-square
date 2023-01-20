using Microsoft.AspNetCore.SignalR;

namespace Training.CircleSquareGame.Api;

public enum Field
{
    Empty,
    X,
    O
}
public enum Status
{
    InGame,
    WonByX,
    WonByO,
    Draw
}

public class XOHub : Hub
{

    public const int BoardSize = 3;
    public static List<List<Field>> State = new();
    public static Field CurrentTurn = Field.O;
    public static Status CurrentStatus = Status.InGame;
    
    public async Task ResetBoard()
    {
        CurrentTurn = Field.O;
        CurrentStatus = Status.InGame;
        State.Clear();
        for (var i = 0; i < BoardSize; i++)
        {
            var row = new List<Field>();
            for (var j = 0; j < BoardSize; j++)
            {
                row.Add(Field.Empty);
            }
            State.Add(row);
        }
        await Clients.All.SendAsync("NewGame");
        await Clients.All.SendAsync("GameStatus", CurrentStatus);
        await Clients.All.SendAsync("CurrentTurn", CurrentTurn);
    }

    public async Task GetCurrentTurn()
    {
        await Clients.All.SendAsync("CurrentTurn", CurrentTurn);
    }

    public async Task GetField(int x, int y)
    {
        if (x is >= 0 and < BoardSize && y is >= 0 and < BoardSize)
        {
            await Clients.All.SendAsync("CurrentFieldValue", x, y, State[x][y]);
        }
    }
    
    public async Task SetField(int x, int y)
    {
        if (x is >= 0 and < BoardSize && y is >= 0 and < BoardSize)
        {
            if (State[x][y] == Field.Empty && CurrentStatus == Status.InGame)
            {
                State[x][y] = CurrentTurn;
                CheckWinCondition();
                CurrentTurn = CurrentTurn == Field.O ? Field.X : Field.O;
                await Clients.All.SendAsync("CurrentTurn", CurrentTurn);
                await Clients.All.SendAsync("CurrentFieldValue", x, y, State[x][y]);
            }
        }
    }
    
    public async void CheckWinCondition()
    {
        List<List<Field>> checkLines = new();
        List<Field> diagonalForward = new();
        List<Field> diagonalBackward = new();
        for (var i = 0; i < BoardSize; i++)
        {
            var row = State[i];
            checkLines.Add(row);

            var column = State.Select(r => r[i]).ToList();
            checkLines.Add(column);
            
            diagonalForward.Add(State[i][i]);
            diagonalBackward.Add(State[i][BoardSize-i-1]);
        }
        checkLines.Add(diagonalForward);
        checkLines.Add(diagonalBackward);

        foreach (var line in checkLines)
        {
            if (line.TrueForAll(field => field != Field.Empty && field == line[0]))
            {
                CurrentStatus = line[0] == Field.O ? Status.WonByO : Status.WonByX;
                await Clients.All.SendAsync("GameStatus", CurrentStatus);
                return;
            }
        }

        if (State.TrueForAll(row => row.TrueForAll(field => field != Field.Empty)))
        {
            CurrentStatus = Status.Draw;
            await Clients.All.SendAsync("GameStatus", CurrentStatus);
        }

    }

}