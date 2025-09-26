cmake_minimum_required(VERSION 4.0)
project(Nudgly)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

set(CMAKE_PREFIX_PATH "/Users/harikeshavr/Qt/6.9.2/macos/lib/cmake")

find_package(Qt6 COMPONENTS
        Core
        Gui
        Quick
        Qml
        REQUIRED)

add_executable(Nudgly
        src/main.cpp
        src/constants.h

        # ---- Models -----
        src/models/ConversationModel.cpp
        src/models/ConversationModel.h

        # ----- Services -----
        src/Services/LLMService.cpp
        src/Services/LLMService.h

        src/Services/SettingsService.cpp
        src/Services/SettingsService.h

        src/services/WindowPrivacyService.mm
        src/services/WindowPrivacyService.h

        # ----- ViewModels -----
        src/ViewModels/MainViewModel.cpp
        src/ViewModels/MainViewModel.h

        # ----- Resource files -----
        src/Views/Views.qrc
)
target_link_libraries(Nudgly
        Qt::Core
        Qt::Gui
        Qt::Quick
        Qt::Qml
)

