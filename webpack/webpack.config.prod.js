const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const pkg = require('../package.json');
const shared = require('./shared.js');

const appName = `${pkg.name}-${pkg.version}`;
const jsBundleName = `${appName}.min.js`;
const cssBundleName = `${appName}.min.css`;
const indexHtmlPath = path.resolve(__dirname, '../static/index.html');

module.exports = {
  devtool: 'sourcemap',
  entry: shared.entry,
  output: {
    path: path.resolve(__dirname, '../dist'),
    filename: `/${jsBundleName}`
  },
  module: Object.assign({}, shared.module, {
    loaders: shared.module.loaders.map((loader) => {
      if (loader.id === 'style') {
        return Object.assign({}, loader, {
          loader: ExtractTextPlugin.extract('style', loader.loader.replace('style', ''))
        });
      }

      return loader;
    })
  }),
  resolve: shared.resolve,
  plugins: [
    new HtmlWebpackPlugin({
      inject: true,
      template: indexHtmlPath,
      minify: {
        removeComments: true,
        collapseWhitespace: true,
        removeRedundantAttributes: true,
        useShortDoctype: true,
        removeEmptyAttributes: true,
        removeStyleLinkTypeAttributes: true,
        keepClosingSlash: true,
        minifyJS: true,
        minifyCSS: true,
        minifyURLs: true
      }
    }),
    new webpack.DefinePlugin({ 'process.env.NODE_ENV': '"production"' }),
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.optimize.DedupePlugin(),
    new webpack.optimize.UglifyJsPlugin({
      compressor: {
        screw_ie8: true,
        warnings: false
      },
      mangle: {
        screw_ie8: true
      },
      output: {
        comments: false,
        screw_ie8: true
      }
    }),
    new ExtractTextPlugin(cssBundleName)
  ]
};
