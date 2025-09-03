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

