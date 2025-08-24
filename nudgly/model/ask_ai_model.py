import base64

from PySide6.QtCore import QBuffer, QByteArray, QIODevice
from PySide6.QtGui import QGuiApplication


class AskAiModel:
    @classmethod
    def takeScreenshot(cls):
        # Get the primary screen
        screen = QGuiApplication.primaryScreen()
        if not screen:
            return None

        # Grab the entire desktop (winId 0 means desktop)
        pixmap = screen.grabWindow(0)

        # Convert QPixmap to base64 PNG
        return cls._toBase64Png(pixmap)

    @staticmethod
    def _toBase64Png(pixmap):
        if pixmap is None:
            return ""

        # Convert QPixmap -> QImage
        image = pixmap.toImage()

        # Write QImage into a buffer as PNG
        buffer = QBuffer()
        buffer.open(QIODevice.WriteOnly)
        image.save(buffer, "PNG")
        byte_array: QByteArray = buffer.data()
        buffer.close()

        # Encode to base64 string
        return base64.b64encode(byte_array.data()).decode("utf-8")

    @classmethod
    def askAi(cls):
        text = "Hello, world!"
        return text.strip()
