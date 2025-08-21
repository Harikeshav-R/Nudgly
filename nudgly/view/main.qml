import QtQuick
import QtQuick.Controls

ApplicationWindow {
    visible: true
    width: 400
    height: 600
    title: "MVVM Todo App"

    Column {
        anchors.centerIn: parent

        TextField {
            id: input
            placeholderText: "New Todo"
        }

        Button {
            text: "Add"
            onClicked: TodoVM.addTodo(input.text)
        }

        ListView {
            width: parent.width
            height: 400
            model: TodoVM.todos

            delegate: Text {
                text: modelData.title
            }
        }
    }
}
