import json

from PySide6.QtCore import QObject, Signal, Slot, QBuffer, QIODevice, QByteArray
from PySide6.QtGui import QGuiApplication, QPixmap
from PySide6.QtNetwork import QNetworkAccessManager, QNetworkRequest, QNetworkReply

from nudgly.constants import Constants
from nudgly.models.conversation_model import Message, TextPart, ImageUrlPart, ConversationModel
from nudgly.services.config_service import ConfigService
from nudgly.services.logging_service import LoggingService


class LLMService(QObject):
    answerGenerated = Signal(str)
    errorOccurred = Signal(str)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._API_KEY = ConfigService.read_value("LLM", "api_key")
        self._network_manager = QNetworkAccessManager(self)

    @staticmethod
    def _to_base64_png(pixmap: QPixmap) -> str | None:
        if pixmap is None:
            return None

        buffer = QBuffer()
        buffer.open(QIODevice.OpenModeFlag.WriteOnly)

        image = pixmap.toImage()
        image.save(buffer, "PNG")

        byte_array = buffer.data()
        buffer.close()

        return byte_array.toBase64().data().decode("utf-8")

    @staticmethod
    def take_screenshot() -> str | None:
        screen = QGuiApplication.primaryScreen()

        if not screen:
            LoggingService.error("Unable to get primary screen.")
            return None

        pixmap = screen.grabWindow(0)

        return LLMService._to_base64_png(pixmap)

    @Slot(ConversationModel)
    def generate_answer(self, conversation_model: ConversationModel):
        base64_encoded_screenshot = self.take_screenshot()

        if not base64_encoded_screenshot:
            error_msg = "Capturing screen failed."
            LoggingService.error(error_msg)
            self.errorOccurred.emit(error_msg)
            return

        user_text_part: TextPart = {
            "type": "text",
            "text": Constants.DEFAULT_SYSTEM_PROMPT,
        }
        user_image_part: ImageUrlPart = {
            "type": "image_url",
            "image_url": {"url": f"data:image/png;base64,{base64_encoded_screenshot}"}
        }
        user_message: Message = {
            "role": "user",
            "content": [user_text_part, user_image_part]
        }

        self._send_api_request(user_message, conversation_model)

    def _send_api_request(self, user_message: Message, conversation_model: ConversationModel):
        conversation_data = conversation_model.get_conversation_data()
        conversation_data["messages"].append(user_message)

        request = QNetworkRequest(Constants.MODEL_ENDPOINT)
        request.setRawHeader(b"Authorization", f"Bearer {self._API_KEY}".encode())
        request.setRawHeader(b"Content-Type", "application/json".encode())

        json_payload = json.dumps(conversation_data).encode('utf-8')
        data = QByteArray(json_payload)

        LoggingService.info("Sending request to Gemini API...")
        reply = self._network_manager.post(request, data)

        # Connect the reply's finished signal to a handler slot.
        # Use a lambda to pass the reply and conversation model to the handler.
        reply.finished.connect(lambda: self._handle_api_response(reply, conversation_model, user_message))

    def _handle_api_response(self, reply: QNetworkReply, conversation_model: ConversationModel,
                             user_message: Message) -> None:
        if reply.error() != QNetworkReply.NetworkError.NoError:
            error_string = reply.errorString()
            response_body = reply.readAll().data().decode('utf-8')
            LoggingService.error(f"Error making API request: {error_string}")
            LoggingService.info(f"Response Body: {response_body}")
            self.errorOccurred.emit(error_string)
        else:
            LoggingService.info("Request successful.")
            response_data = reply.readAll().data()
            try:
                output = json.loads(response_data)

                conversation_model.add_message(user_message["role"], user_message["content"])

                assistant_text = output["choices"][0]["message"]["content"]
                assistant_message: Message = {
                    "role": output["choices"][0]["message"]["role"],
                    "content": [{"type": "text", "text": assistant_text}]
                }
                conversation_model.add_message(assistant_message["role"], assistant_message["content"])

                self.answerGenerated.emit(assistant_text)

            except (json.JSONDecodeError, KeyError) as e:
                error_msg = f"Error parsing API response: {e}"
                LoggingService.error(error_msg)
                self.errorOccurred.emit(error_msg)

        reply.deleteLater()
