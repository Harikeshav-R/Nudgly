from PySide6.QtCore import QObject, Signal, Slot, Property

from nudgly.model.ask_ai_model import AskAiModel


class MainViewModel(QObject):
    textResultChanged = Signal()
    windowVisibilityChanged = Signal()

    def __init__(self):
        super().__init__()
        self._textResult = ""
        self._windowVisible = True

    @Slot()
    def askAi(self):
        pixmap = AskAiModel.takeScreenshot()
        text = AskAiModel.askAi()
        self._textResult = text
        self.textResultChanged.emit()

    @Slot()
    def toggleWindowVisibility(self):
        self._windowVisible = not self._windowVisible
        self.windowVisibilityChanged.emit()

    @Slot()
    def openSettings(self):
        print("Settings window requested")

    @Property(str, notify=textResultChanged)
    def textResult(self):
        return self._textResult

    @Property(bool, notify=windowVisibilityChanged)
    def windowVisible(self):
        return self._windowVisible
