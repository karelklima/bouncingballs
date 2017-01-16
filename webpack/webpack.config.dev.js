const { resolve } = require('path')
const { merge } = require('lodash')
const webpack = require('webpack')
const HtmlWebpackPlugin = require('html-webpack-plugin')
const pkg = require('../package.json')

const webpackConfigCommon = require('./webpack.config.common.js')

const bundleName = `${pkg.name}-${pkg.version}.js`
const indexHtmlPath = resolve(__dirname, '../src/index.html')
const port = process.env.PORT || 3000

const webpackConfigDev = {
  devtool: 'sourcemap',
  entry: {
    webpack: [
      'react-hot-loader/patch',
      `webpack-dev-server/client?http://localhost:${port}`,
      'webpack/hot/only-dev-server'
    ]
  },
  output: {
    path: resolve(__dirname, '../'),
    filename: '[hash].[name].js'
  },
  devServer: {
    hot: true,
    port: port,
    inline: true,
    contentBase: resolve(__dirname, '../static'),
    historyApiFallback: true
  },
  plugins: [
    new HtmlWebpackPlugin({
      inject: true,
      template: indexHtmlPath
    }),
    new webpack.optimize.CommonsChunkPlugin({
      names: ['vendor', 'manifest'] // Specify the common bundle's name.
    }),
    new webpack.HotModuleReplacementPlugin(), // enable HMR globally
    new webpack.NamedModulesPlugin(), // prints more readable module names in the browser console on HMR updates
  ]
}

module.exports = merge({}, webpackConfigCommon, webpackConfigDev)
