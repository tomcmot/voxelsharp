#version 330 core

layout (location = 0) in vec3 aPosition;

// On top of our aPosition attribute, we now create an aTexCoords attribute for our texture coordinates.
layout (location = 1) in vec2 aTexCoords;

// Likewise, we also assign an out attribute to go into the fragment shader.
out vec2 frag_texCoords;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // note that we read the multiplication from right to left
    gl_Position = projection * view * model * vec4(aPosition, 1.0);

    // This basic vertex shader does no additional processing of texture coordinates, so we can pass them
    // straight to the fragment shader.
    frag_texCoords = aTexCoords;
}