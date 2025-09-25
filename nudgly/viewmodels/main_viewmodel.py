from PySide6.QtCore import QObject, Signal, Slot, Property

from nudgly.models.conversation_model import ConversationModel
from nudgly.services.llm_service import LLMService


class MainViewModel(QObject):
    answerResultChanged = Signal()
    answersWindowVisibilityChanged = Signal()
    isThinkingChanged = Signal()

    def __init__(self, conversation_model: ConversationModel):
        super().__init__()

        self._conversation_model = conversation_model

        # --- Properties ---
        self._answerResult: str = ""
        self._answersWindowVisible: bool = False
        self._isThinking: bool = False

    # --- Slots ---
    @Slot()
    def ask_ai(self):
        self.isThinking = True

        assistant_response = LLMService.generate_answer(
            self._conversation_model.get_conversation_data(),
        )
        self.answerResult = assistant_response

        self.isThinking = False

    @Slot()
    def toggle_answers_window_visibility(self):
        self.answersWindowVisible = not self.answersWindowVisible

    @Slot()
    def open_settings(self):
        print("Settings window requested")

    # --- Property: answerResult ---
    def get_answer_result(self):
        return self._answerResult

    def set_answer_result(self, value):
        if self._answerResult != value:
            self._answerResult = value
            self.answerResultChanged.emit()

    answerResult = Property(str, get_answer_result, set_answer_result, notify=answerResultChanged)

    # --- Property: answersWindowVisible ---
    def get_answers_window_visible(self):
        return self._answersWindowVisible

    def set_answers_window_visible(self, value):
        if self._answersWindowVisible != value:
            self._answersWindowVisible = value
            self.answersWindowVisibilityChanged.emit()

    answersWindowVisible = Property(bool, get_answers_window_visible, set_answers_window_visible,
                                    notify=answersWindowVisibilityChanged)

    # --- Property: isThinking ---
    def get_is_thinking(self):
        return self._isThinking

    def set_is_thinking(self, value):
        if self._isThinking != value:
            self._isThinking = value
            self.isThinkingChanged.emit()

    isThinking = Property(bool, get_is_thinking, set_is_thinking, notify=isThinkingChanged)
