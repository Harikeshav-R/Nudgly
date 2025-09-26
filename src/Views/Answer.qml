import QtQuick
import QtQuick.Controls.FluentWinUI3

ApplicationWindow {
    id: answersWindow

    color: "#65000000"
    flags: Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint | Qt.WindowTransparentForInput
    height: Screen.height * 0.8
    maximumHeight: height
    maximumWidth: width
    minimumHeight: height
    minimumWidth: width
    visible: mainViewModel.answersWindowVisible
    width: Screen.width * 0.8
    x: Screen.width / 2 - width / 2
    y: Screen.width / 2 - width / 2

    ScrollView {
        id: answersScrollView

        anchors.fill: parent
        clip: true

        TextArea {
            id: answerText

            color: "white"
            font.pointSize: 16
            height: contentHeight
            readOnly: true
            text: mainViewModel.answerResult
            textFormat: TextEdit.MarkdownText
            textMargin: 20
            width: answersScrollView.width
            wrapMode: Text.Wrap
        }
    }
}