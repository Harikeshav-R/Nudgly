import base64

import requests
from PySide6.QtCore import QBuffer, QIODevice
from PySide6.QtGui import QGuiApplication, QPixmap

from nudgly.constants import Constants
from nudgly.models.conversation_model import Conversation, Message, TextPart, APIResponse, ImageUrlPart
from nudgly.services.config_service import ConfigService
from nudgly.services.logging_service import LoggingService


class LLMService:
    _API_KEY = ConfigService.read_value("LLM", "api_key")

    @staticmethod
    def _to_base64_png(pixmap: QPixmap) -> str | None:
        if pixmap is None:
            return None

        # Convert QPixmap -> QImage
        image = pixmap.toImage()

        # Write QImage into a buffer as PNG
        buffer = QBuffer()
        buffer.open(QIODevice.WriteOnly)
        image.save(buffer, "PNG")
        byte_array = buffer.data()
        buffer.close()

        # Encode to base64 string
        return base64.b64encode(bytes(byte_array.data())).decode("utf-8")

    @staticmethod
    def take_screenshot() -> str | None:
        # Get the primary screen
        screen = QGuiApplication.primaryScreen()

        if not screen:
            LoggingService.error("Unable to get primary screen.")
            return None

        # Grab the entire desktop (winId 0 means desktop)
        pixmap = screen.grabWindow(0)

        # Convert QPixmap to base64 PNG
        return LLMService._to_base64_png(pixmap)

    @classmethod
    def send_api_request(
            cls,
            conversation: Conversation,
    ) -> APIResponse | None:
        headers = {
            'x-goog-api-key': cls._API_KEY,
            'Content-Type': 'application/json'
        }

        LoggingService.info("Sending request to Gemini API...")
        try:
            response = requests.post(Constants.MODEL_ENDPOINT, headers=headers, json=conversation)
            response.raise_for_status()
            LoggingService.info("Request successful.")
            output: APIResponse = response.json()

            return output

        except requests.exceptions.RequestException as e:
            LoggingService.error(f"Error making API request: {e}")

            if e.response:
                LoggingService.info(f"Response Body: {e.response.text}")

            return None

    @classmethod
    def generate_answer(cls, conversation: Conversation) -> None:
        screenshot = cls.take_screenshot()

        if not screenshot:
            LoggingService.error("Capturing screen failed.")
            return

        user_text_part: TextPart = {
            "type": "text",
            "text": "Here is the attached image.",
        }

        user_image_part: ImageUrlPart = {
            "type": "image_url",
            "image_url": {
                "url": f"data:image/png;base64,{screenshot}"
            }
        }

        user_message: Message = {
            "role": "user",
            "content": [
                user_text_part,
                user_image_part
            ]
        }

        conversation["messages"].append(user_message)

        output = cls.send_api_request(conversation)

        assistant_text_part: TextPart = {
            "type": "text",
            "text": output["choices"][0]["message"]["content"],
        }

        assistant_message: Message = {
            "role": output["choices"][0]["message"]["role"],
            "content": [assistant_text_part]
        }

        conversation["messages"].append(assistant_message)
