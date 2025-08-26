import QtQuick
import QtQuick.Controls.FluentWinUI3
import QtQuick.Layouts

ApplicationWindow {
    id: commandBar
    visible: true
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    color: "transparent"

    width: mainWindowBackground.implicitWidth
    height: mainWindowBackground.implicitHeight

    maximumWidth: width
    maximumHeight: height

    minimumWidth: width
    minimumHeight: height

    Component.onCompleted: {
        commandBar.x = commandBar.screen.virtualX + (commandBar.screen.width - commandBar.width) / 2;
        commandBar.y = 0
    }

    Rectangle {
        id: mainWindowBackground

        anchors.fill: parent
        anchors.centerIn: parent

        color: "#15000000"

        radius: 16
        opacity: 0.8

        implicitWidth: row.implicitWidth + 32
        implicitHeight: row.implicitHeight + 16

        RowLayout {
            id: row

            anchors {
                fill: parent
                leftMargin: 16
                rightMargin: 16
                topMargin: 8
                bottomMargin: 8
            }
            spacing: 20

            Button {
                id: askAiButton
                text: "Ask AI"
                font.pointSize: 14
                onClicked: mainVM.askAi()
            }

            Button {
                id: toggleAnswersButton
                text: "Show/Hide Answers"
                font.pointSize: 14
                onClicked: mainVM.toggleAnswersWindowVisibility()
            }

            Button {
                id: settingsButton
                text: "Settings"
                font.pointSize: 14
                onClicked: mainVM.openSettings()
            }
        }
    }
}
