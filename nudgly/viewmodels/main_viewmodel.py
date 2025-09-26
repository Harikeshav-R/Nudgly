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

        # --- Instantiate the service ---
        self._llm_service = LLMService(self)

        # --- Connect service signals to local slots ---
        self._llm_service.answerGenerated.connect(self._on_answer_received)
        self._llm_service.errorOccurred.connect(self._on_api_error)

        # --- Properties ---
        self._answerResult: str = ""
        self._answersWindowVisible: bool = False
        self._isThinking: bool = False

    # --- Public Slots (callable from UI) ---
    @Slot()
    def askAi(self):
        """
        This slot is now non-blocking. It starts the AI task and returns immediately.
        """
        self.isThinking = True
        # Call the asynchronous method from the service
        self._llm_service.generate_answer(self._conversation_model)

    @Slot()
    def toggleAnswersWindowVisibility(self):
        self.answersWindowVisible = not self.answersWindowVisible

    @Slot()
    def openSettings(self):
        print("Settings window requested")

    # --- Private Slots (for handling service signals) ---
    @Slot(str)
    def _on_answer_received(self, assistant_response: str):
        """Handles the successful result from the LLMService."""
        self.answerResult = assistant_response
        self.isThinking = False

    @Slot(str)
    def _on_api_error(self, error_message: str):
        """Handles errors from the LLMService."""
        # You might want to format this for the user
        self.answerResult = f"An error occurred: {error_message}"
        self.isThinking = False

    # --- Property Definitions (unchanged) ---
    def get_answer_result(self):
        return self._answerResult

    def set_answer_result(self, value):
        if self._answerResult != value:
            self._answerResult = value
            self.answersWindowVisible = True
            self.answerResultChanged.emit()

    answerResult = Property(str, get_answer_result, set_answer_result, notify=answerResultChanged)

    def get_answers_window_visible(self):
        return self._answersWindowVisible

    def set_answers_window_visible(self, value):
        if self._answersWindowVisible != value:
            self._answersWindowVisible = value
            self.answersWindowVisibilityChanged.emit()

    answersWindowVisible = Property(bool, get_answers_window_visible, set_answers_window_visible,
                                    notify=answersWindowVisibilityChanged)

    def get_is_thinking(self):
        return self._isThinking

    def set_is_thinking(self, value):
        if self._isThinking != value:
            self._isThinking = value
            self.isThinkingChanged.emit()

    isThinking = Property(bool, get_is_thinking, set_is_thinking, notify=isThinkingChanged)
