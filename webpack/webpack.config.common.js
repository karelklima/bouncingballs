const { resolve } = require('path')

const cssnext = require('postcss-cssnext')

src = resolve(__dirname, '../src')

module.exports = {
  entry: {
    main: [
      './src/index.tsx'
    ],
    vendor: [
      'react'
    ]
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: [
          {
            loader: 'awesome-typescript-loader'
          }
        ],
        include: [
          src
        ]
      },
      {
        test: /\.jsx?$/,
        use: [
          {
            loader: 'awesome-typescript-loader',
            options: {
              useBabel: true
            }
          }
        ],
        include: [
          src
        ]
      },
      {
        test: /\.css$/,
        use: [
          {
            loader: 'style-loader'
          },
          {
            loader: 'css-loader',
            options: {
              autoprefixer: false,
            }
          },
          {
            loader: 'postcss-loader',
            options: {
              plugins: {
                postcss: cssnext
              }
            }
          }
        ]
      }
    ]
  },
  resolve: {
    extensions: ['.js', '.json', '.jsx', '.ts', '.tsx', '.css'],
    modules: [
      'node_modules',
      src
    ]
  },

}