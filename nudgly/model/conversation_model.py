import logging

from typing import Optional

from google.genai import chats
from PySide6.QtCore import (
    QAbstractListModel,
    QModelIndex,
    Qt,
    QObject,
)

logger = logging.getLogger(__name__)


class ConversationModel(QAbstractListModel):
    RoleRole = Qt.UserRole + 1
    TextContentRole = Qt.UserRole + 2
    ImageContentRole = Qt.UserRole + 3

    def __init__(self, chat:chats.Chat, parent: Optional[QObject] = None):
        super().__init__(parent)

        self.chat = chat
        self._history = []

        logger.debug(f"Initialized ConversationModel.")

    def rowCount(self, parent=QModelIndex()) -> int:
        return len(self._history)

    def data(self, index: QModelIndex, role: int = Qt.DisplayRole):
        if not index.isValid() or not (0 <= index.row() < len(self._history)):
            return None

        item = self._history[index.row()]

        if role == self.RoleRole:
            return item.get("role")

        elif role == self.TextContentRole:
            for part in item.get("parts", []):
                if part.get("text") and not part.get("inline_data"):
                    return part["text"]
            return None

        elif role == self.ImageContentRole:
            for part in item.get("parts", []):
                if part.get("inline_data") and not part.get("text"):
                    return part["inline_data"].get("data")
            return None

        return None

    def roleNames(self):
        return {
            self.RoleRole: b"role",
            self.TextContentRole: b"textContent",
            self.ImageContentRole: b"imageContent",
        }

    def add_messages(self):
        self.beginResetModel()
        self._history = self.chat.get_history()
        self.endResetModel()

        logger.info(f"Added user and model messages to conversation history")
