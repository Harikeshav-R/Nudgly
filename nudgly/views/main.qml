import QtQuick 2.15
import QtQuick.Window 2.15
import QtQuick.Controls.FluentWinUI3 2.15
import QtQuick.Layouts 1.15

ApplicationWindow {
    id: mainWindow

    color: "#55000000"
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    height: buttonRow.height + 30
    maximumHeight: height
    maximumWidth: width
    minimumHeight: height
    minimumWidth: width
    visible: true
    width: Screen.width * 0.4
    x: (Screen.width - width) / 2
    y: 0

    RowLayout {
        id: buttonRow

        anchors.centerIn: parent
        spacing: 10
        width: parent.width * 0.95

        Button {
            id: askAiButton

            Layout.fillWidth: true
            text: "Ask AI"

            onClicked: mainViewModel.askAi()
        }
        Button {
            id: toggleWindowButton

            Layout.fillWidth: true
            text: "Toggle Window"

            onClicked: mainViewModel.toggleAnswersWindowVisibility()
        }
        Button {
            id: toggleInvisibilityButton

            Layout.fillWidth: true
            text: "Toggle Invisibility"
        }
        Button {
            id: settingsButton

            Layout.fillWidth: true
            text: "Settings"

            onClicked: mainViewModel.openSettings()
        }
    }
}
