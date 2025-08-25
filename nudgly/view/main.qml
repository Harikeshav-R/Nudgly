import QtQuick
import QtQuick.Controls.FluentWinUI3
import QtQuick.Layouts

ApplicationWindow {
    id: overlay
    visible: true
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    color: "transparent"

    width: background.implicitWidth
    height: background.implicitHeight

    Rectangle {
        id: background
        anchors.centerIn: parent
        radius: 16
        color: "#15000000"
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
