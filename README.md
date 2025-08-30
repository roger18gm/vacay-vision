# VacayVision

Published URL: https://vacay-vision.onrender.com/

## Visualize Your Dream Vacation, Together!

VacayVision is a web application designed to help users visualize and plan their dream vacations, even if they don't know where to start. It provides a platform for users to create personalized vacation "widgets" (HTML div cards) where they can input details like desired locations, essential items to bring, and must-visit hotspots. The core idea is to foster a community where travelers can share their dream itineraries, inspire others, and make travel planning a collaborative and easy experience.

## 🎯 Goal

The primary goal of VacayVision is to:

- Empower Users: Provide a user-friendly interface for individuals to meticulously plan and visualize their ideal getaways.

- Inspire Travel: Create a vibrant community page where users can discover new destinations, activities, and packing essentials through shared dream vacations.

- Simplify Planning: Offer a centralized hub for travel inspiration, helping users who are unsure about potential travel destinations or what to expect.

- Facilitate Sharing: Enable seamless sharing of personalized vacation plans, fostering a sense of community among travelers.

## 🗺️ Plan

The development of VacayVision will follow these key phases:

- User Authentication: Implement robust user registration, login, and logout functionalities to secure user data and personalize experiences.

- Personalized Vacation Creation: Develop the core feature allowing users to create custom vacation "widgets" (HTML div cards). These widgets will support input fields for:

  1. Location details

  2. Items to bring

  3. Hotspots/Attractions

  4. Notes and descriptions

- User Profiles: Create individual user profiles where registered users can view and manage their created vacations. Other users will also be able to view public profiles.

- Community Vacation Page: Establish a dedicated community page to showcase approved dream vacations from various users.

- Admin Moderation: Implement an administrative role with the ability to review, approve, or reject user-submitted vacation plans for the community page, ensuring quality and relevance.

- Search and Discovery: (Future consideration) Implement search and filtering capabilities on the community page to help users find specific types of vacations or destinations.

## 💻 Tech Stack

VacayVision will be built as a server-side rendered (SSR) application using the following technologies:

### Frontend:

- HTML: For structuring the web content.

- CSS: For styling and ensuring a visually appealing and responsive user interface.

- JavaScript: For interactive elements and client-side logic.

- EJS (Embedded JavaScript): As the templating engine for rendering dynamic HTML on the server.

### Backend:

- Node.js: The JavaScript runtime environment.

- Express.js: A fast, unopinionated, minimalist web framework for Node.js, used for building the API and handling routing.

### Database:

- PostgreSQL: A powerful, open-source relational database system for storing user data, vacation details, and administrative information.

### Helpful dev commands

pnpm instead of npm

pnpm run dev for live websocket reload

### AI Agent Resources

https://ai-sdk.dev/docs/foundations/providers-and-models

https://ai-sdk.dev/providers/community-providers/ollama

https://github.com/openai/openai-agents-js

https://openai.github.io/openai-agents-js/guides/running-agents/
