import QtQuick.Window
import QtQuick.Controls.FluentWinUI3
import QtQuick.Layouts

ApplicationWindow {
    id: window

    color: "#55000000"
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    height: buttonRow.height + 30
    maximumHeight: height
    maximumWidth: width
    minimumHeight: height
    minimumWidth: width
    title: "Nudgly"
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
        }
        Button {
            id: toggleWindowButton

            Layout.fillWidth: true
            text: "Toggle Window"
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
        }
    }
}