import QtQuick
import QtQuick.Controls.FluentWinUI3

ApplicationWindow {
    id: answersWindow

    color: "#55000000"
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint
    height: Screen.height * 0.75
    maximumHeight: height
    maximumWidth: width
    minimumHeight: height
    minimumWidth: width
    visible: mainViewModel.answersWindowVisible
    width: Screen.width * 0.75
    x: Screen.width / 2 - width / 2
    y: Screen.width / 2 - width / 2

    ScrollView {
        id: answersScrollView

        anchors.fill: parent
        clip: true

        TextArea {
            id: answerText

            color: "white"
            height: contentHeight
            readOnly: true
            text: mainViewModel.answerResult
            textFormat: TextEdit.MarkdownText
            width: answersScrollView.width
            wrapMode: Text.Wrap
        }
    }
}