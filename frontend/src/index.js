import '@riotjs/hot-reload';
import { mount, register, install } from 'riot';
import Hub from './hub';

import GameBoard from './components/game-board/game-board.riot';
import OxField from './components/ox-field/ox-field.riot';
import NewGameButton from './components/new-game-button/new-game-button.riot';
import HeaderText from './components/header-text/header-text.riot';

var appState = {
    boardSize: 3,
    board: []
};

// SignalR
var hub = new Hub("http://localhost:5000/xohub");

// ----------------------------------
// SignalR calls from backend go here

hub.connection.on("CurrentFieldValue", (x, y, value) => {
    appState.board[x][y].update({value});
});

hub.connection.on("NewGame", () => {
    for (let i = 0; i < appState.boardSize; i++) {
        for (let j = 0; j < appState.boardSize; j++) {
            appState.board[i][j].update({value: 0});
        }
    }
});

hub.connection.on("CurrentTurn", (value) => {
    appState.header.update({currentTurn: value});
});

hub.connection.on("GameStatus", (value) => {
    appState.header.update({currentStatus: value});
});

// SignalR calls from backend go here
// ----------------------------------

// RiotJs
install(function(component) {
    component.appState = appState;
    component.hub = hub.connection;
});

// -----------------------------------------------
// RiotJs component registration happens here here

register('ox-field', OxField);
register('game-board', GameBoard);
register('new-game-button', NewGameButton);
register('header-text', HeaderText);

// RiotJs component registration happens here here
// -----------------------------------------------

async function StartApp() {
    for (let i = 0; i < appState.boardSize; i++) {
        const row = [];
        for (let j = 0; j < appState.boardSize; j++) {
            row.push(0);
        }
        appState.board.push(row);
    }
    await hub.start();
    mount('game-board');
}

StartApp();
