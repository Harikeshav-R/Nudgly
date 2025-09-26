import QtQuick
import QtQuick.Controls.FluentWinUI3

ApplicationWindow {
    id: answersWindow

    width: Screen.width * 0.75
    height: Screen.height * 0.75

    maximumWidth: width
    maximumHeight: height

    minimumWidth: width
    minimumHeight: height

    x: Screen.width / 2 - width / 2
    y: Screen.width / 2 - width / 2

    Component.onCompleted: {
        answersWindow.x = answersWindow.screen.virtualX + (answersWindow.screen.width - answersWindow.width) / 2;
        answersWindow.y = answersWindow.screen.virtualY + (answersWindow.screen.height - answersWindow.height) / 2;
    }

    visible: mainVM.answersWindowVisible

    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    color: "transparent"

    Rectangle {
        anchors.fill: parent
        anchors.centerIn: parent

        color: "#45000000"

        radius: 16
        opacity: 0.95

        ScrollView {
            id: answersScrollView
            anchors.fill: parent
            clip: true

            TextArea {
                id: answerText

                width: answersScrollView.width
                height: contentHeight

                color: "white"
                wrapMode: Text.Wrap
                readOnly: true

                text: mainVM.answerResult
            }
        }
    }
}