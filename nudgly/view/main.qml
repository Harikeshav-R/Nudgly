import QtQuick
import QtQuick.Controls
import QtQuick.Layouts

ApplicationWindow {
    id: root
    width: 600
    height: 400
    visible: mainVM.windowVisible
    title: "Nudgly"

    ColumnLayout {
        anchors.fill: parent

        TabBar {
            Layout.fillWidth: true

            TabButton {
                text: "Ask AI"
                onClicked: mainVM.takeScreenshot()
            }

            TabButton {
                text: "Show/Hide"
                onClicked: mainVM.toggleWindowVisibility()
            }

            TabButton {
                text: "Settings"
                onClicked: mainVM.openSettings()
            }
        }

        ScrollView {
            Layout.fillWidth: true
            Layout.fillHeight: true

            TextArea {
                readOnly: true
                wrapMode: Text.Wrap
                text: mainVM.textResult
            }
        }
    }
}
