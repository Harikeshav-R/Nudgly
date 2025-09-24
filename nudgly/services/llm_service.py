from typing import Optional

from PySide6.QtCore import QBuffer, QIODevice
from PySide6.QtGui import QGuiApplication, QPixmap

from nudgly.services.config_service import ConfigService


class LLMService:
    _API_KEY = ConfigService.read_value("LLM", "api_key")

    @staticmethod
    def _to_png(pixmap: QPixmap):
        if pixmap is None:
            return ""

        # Convert QPixmap -> QImage
        image = pixmap.toImage()

        # Write QImage into a buffer as PNG
        buffer = QBuffer()
        buffer.open(QIODevice.WriteOnly)
        image.save(buffer, "PNG")
        byte_array = buffer.data()
        buffer.close()

        # Encode to base64 string
        return bytes(byte_array.data())

    @staticmethod
    def take_screenshot():
        # Get the primary screen
        screen = QGuiApplication.primaryScreen()
        if not screen:
            return None

        # Grab the entire desktop (winId 0 means desktop)
        pixmap = screen.grabWindow(0)

        # Convert QPixmap to base64 PNG
        return LLMService._to_png(pixmap)

    @classmethod
    def generate_answer(cls, chat_history: dict, prompt: Optional[str], image_data: Optional[bytes]) -> None:
        message_contents = []

        if image_data:
            message_contents.append(
                types.Part.from_bytes(
                    data=image_data,
                    mime_type='image/png',
                )
            )

        if prompt:
            message_contents.append(prompt)

        response = chat.send_message(message_contents)

        return response.text
