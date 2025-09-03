import base64
import logging

from typing import Any, Optional

from PySide6.QtCore import (
    QAbstractListModel,
    QBuffer,
    QIODevice,
    QModelIndex,
    Qt,
    QObject,
)
from PySide6.QtGui import QGuiApplication, QPixmap

from nudgly.utils.constants import Constants

logger = logging.getLogger(__name__)


class ConversationModel(QAbstractListModel):
    RoleRole = Qt.UserRole + 1
    ContentRole = Qt.UserRole + 2

    def __init__(self, custom_user_instructions: str, parent: Optional[QObject] = None):
        super().__init__(parent)
        system_prompt = Constants.DEFAULT_SYSTEM_PROMPT + "\n" + custom_user_instructions
        self._history: list[dict[str, Any]] = [
            {"role": "system", "content": system_prompt, "image_data": None}
        ]
        logger.debug(f"Initialized ConversationModel with system prompt: {custom_user_instructions!r}")

    def rowCount(self, parent=QModelIndex()) -> int:
        return len(self._history)

    def data(self, index: QModelIndex, role: int = Qt.DisplayRole):
        if not index.isValid() or not (0 <= index.row() < len(self._history)):
            return None

        item = self._history[index.row()]

        if role == self.RoleRole:
            return item["role"]
        elif role == self.ContentRole:
            return item["content"]
        return None

    def roleNames(self):
        return {
            self.RoleRole: b"role",
            self.ContentRole: b"content",
        }

    @staticmethod
    def _to_base64_png(pixmap: QPixmap):
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
        return base64.b64encode(byte_array.data()).decode("utf-8")

    @staticmethod
    def take_screenshot():
        # Get the primary screen
        screen = QGuiApplication.primaryScreen()
        if not screen:
            return None

        # Grab the entire desktop (winId 0 means desktop)
        pixmap = screen.grabWindow(0)

        # Convert QPixmap to base64 PNG
        return ConversationModel._to_base64_png(pixmap)

    def add_user_message(self, message: str):
        self.beginInsertRows(QModelIndex(), len(self._history), len(self._history))
        self._history.append(
            {
                "role": "user",
                "content": message,
                "image_data": self.take_screenshot(),
            }
        )
        self.endInsertRows()
        logger.info(f"Added user message - message number: {len(self._history)}")

    def add_assistant_message(self, message: str):
        self.beginInsertRows(QModelIndex(), len(self._history), len(self._history))
        self._history.append(
            {
                "role": "assistant",
                "content": message,
                "image_data": None
            }
        )
        self.endInsertRows()
        logger.info(f"Added assistant message - message number: {len(self._history)}")