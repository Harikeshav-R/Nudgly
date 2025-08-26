from PySide6.QtCore import QObject, Signal, Slot, Property

from nudgly.model.ask_ai_model import AskAiModel


class MainViewModel(QObject):
    answerResultChanged = Signal()
    answersWindowVisibilityChanged = Signal()

    def __init__(self):
        super().__init__()
        self._answerResult = ""
        self._answersWindowVisible = False

    # --- Slots ---
    @Slot()
    def askAi(self):
        pixmap = AskAiModel.takeScreenshot()
        text = AskAiModel.askAi()
        self.answerResult = text  # Use the setter instead of direct assignment

    @Slot()
    def toggleAnswersWindowVisibility(self):
        self.answersWindowVisible = not self.answersWindowVisible  # Use setter

    @Slot()
    def openSettings(self):
        print("Settings window requested")

    # --- Property: answerResult ---
    def getAnswerResult(self):
        return self._answerResult

    def setAnswerResult(self, value):
        if self._answerResult != value:
            self._answerResult = value
            self.answerResultChanged.emit()

    answerResult = Property(str, getAnswerResult, setAnswerResult, notify=answerResultChanged)

    # --- Property: answersWindowVisible ---
    def getAnswersWindowVisible(self):
        return self._answersWindowVisible

    def setAnswersWindowVisible(self, value):
        if self._answersWindowVisible != value:
            self._answersWindowVisible = value
            self.answersWindowVisibilityChanged.emit()

    answersWindowVisible = Property(bool, getAnswersWindowVisible, setAnswersWindowVisible,
                                    notify=answersWindowVisibilityChanged)
